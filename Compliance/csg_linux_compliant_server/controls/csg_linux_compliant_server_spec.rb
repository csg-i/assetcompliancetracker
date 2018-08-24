title 'CSG Linux Compliant Server Spec'

os_name = attribute('os_name', default: '', description: 'The full name of the Linux OS')
os_version = attribute('os_version', default: '', description: 'The Version of the Linux OS')
tcp_ports = attribute('tcp_ports', default: [], description: 'The allowed list of listening TCP ports')
udp_ports = attribute('udp_ports', default: [], description: 'The allowed list of listening UDP ports')
tcp6_ports = attribute('tcp6_ports', default: [], description: 'The allowed list of listening TCP V6 ports')
udp6_ports = attribute('udp6_ports', default: [], description: 'The allowed list of listening UDP V6  ports')
packages = attribute('packages', default: [], description: 'The allowed list of packages')
ignore_uninstalled_packages = attribute('ignore_uninstalled_packages', default: false, description: 'Ignore failures if all the packages listed are NOT installed')

control 'csg-linux-1' do
  title 'CSG-Linux-1- Verify OS Version'
  desc "Verify the OS Version is #{os_name}"
  impact 1.0 # This is critical
  describe os[:name] do
    it { should eq os_name }
  end
  unless os_version.to_s.empty?
    describe os[:release] do
      it { should eq os_version }
    end
  end
end

# packages
control 'csg-linux-7.3' do
  title 'CSG-Linux-7.3 Verify unspecifed packages are NOT installed'
  desc "Verify unspecifed packages are NOT installed."
  impact 1.0 # This is critical
  package_nots = linux_package_helper.installed_packages.reject { |e| packages.include? e }
  describe linux_package_helper do
    package_nots.each do |f|
      its('installed_packages') { should_not contain_one f }
    end
  end
end

unless ignore_uninstalled_packages
  control 'csg-linux-7.4' do
    title 'CSG-Linux-7.4 Verify specifed packages are installed'
    desc "Verify specifed packages are, in fact, installed."
    impact 1.0 # This is critical
    describe linux_package_helper do
      packages.each do |f|
        its('installed_packages') { should contain_one f }
      end
    end
  end
end

# ports
control 'csg-linux-12' do
  title 'CSG-Linux-12 Verify only specifed ports are listening'
  desc "Verify only specifed ports are listening."
  impact 1.0 # This is critical
  tcp_nots = linux_port_helper.listening_tcp_ports.reject { |p| tcp_ports.include? p[:port] }
  tcp6_nots = linux_port_helper.listening_tcp6_ports.reject { |p| tcp6_ports.include? p[:port] }
  udp_nots = linux_port_helper.listening_udp_ports.reject { |p| udp_ports.include? p[:port] }
  udp6_nots = linux_port_helper.listening_udp6_ports.reject { |p| udp6_ports.include? p[:port] }
  describe linux_port_helper do
    tcp_nots.each do |p|
      its('listening_tcp_ports') { should_not contain_port p }
    end
    tcp6_nots.each do |p|
      its('listening_tcp6_ports') { should_not contain_port p }
    end
    udp_nots.each do |p|
      its('listening_udp_ports') { should_not contain_port p }
    end
    udp6_nots.each do |p|
      its('listening_udp6_ports') { should_not contain_port p }
    end
  end
end
