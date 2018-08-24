class LinuxPortHelper < Inspec.resource(1)
  name 'linux_port_helper'
  desc 'Contains Helper info to get all listening ports'
  example "
    linux_port_helper.listening_ports.each do |pkg|
      ...
    end
    linux_package_helper do
      its('listening_tcp_ports') { should contain_one 22 }
      its('listening_tcp6_ports') { should contain_one 22 }
      its('listening_udp_ports') { should contain_one 22 }
      its('listening_udp6_ports') { should contain_one 22 }
    end
  "
  def initialize()
    return skip_resource 'The `linux_port_helper` resource is not supported on your OS.' unless inspec.os.redhat?
  end
  @@cache = nil

  def listening_tcp_ports(_provider = nil, _version = nil)
      info[:tcp]
  end

  def listening_tcp6_ports(_provider = nil, _version = nil)
      info[:tcp6]
  end

  def listening_udp_ports(_provider = nil, _version = nil)
      info[:udp]
  end

  def listening_udp6_ports(_provider = nil, _version = nil)
      info[:udp6]
  end

  def info
    if @@cache.nil?
      all = Inspec::Resources::LinuxPorts.new(inspec).info
      @@cache = {
        tcp: all.select { |p| p['protocol'] == 'tcp' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process:p['process'] } }.uniq,
        tcp6: all.select { |p| p['protocol'] == 'tcp6' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process:p['process'] } }.uniq,
        udp: all.select { |p| p['protocol'] == 'udp' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process:p['process'] } }.uniq,
        udp6: all.select { |p| p['protocol'] == 'udp6' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process:p['process'] } }.uniq
      }
    end
    @@cache
  end

  def to_s
    'Linux Listening Port Helper'
  end
end
