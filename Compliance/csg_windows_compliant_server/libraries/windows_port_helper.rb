class WindowsPortHelper < Inspec.resource(1)
  name 'windows_port_helper'
  desc 'Contains Helper info to get all listening ports'
  example "
    windows_port_helper.listening_ports.each do |pkg|
      ...
    end
  "
  def initialize()
    return skip_resource 'The `windows_port_helper` resource is not supported on your OS.' unless inspec.os.windows?
  end
  @@cache = nil

  def listening_tcp_ports(_provider = nil, _version = nil)
      info[:tcp]
  end

  def listening_udp_ports(_provider = nil, _version = nil)
      info[:udp]
  end

  def info
    if @@cache.nil?
      all = Inspec::Resources::WindowsPorts.new(inspec).info
      @@cache = {
        tcp: all.select { |p| p['protocol'] == 'tcp' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process:p['process'] } }.uniq,
        udp: all.select { |p| p['protocol'] == 'udp' && p['address'] != '::1' && !p['address'].start_with?('127.0.0.') }.collect { |p| { port: p['port'], process: p['process'] } }.uniq
      }
    end
    @@cache
  end

  def to_s
    'Windows Listening Port Helper'
  end
end
