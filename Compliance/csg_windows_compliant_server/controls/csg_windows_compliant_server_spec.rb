title 'CSG Windows Compliant Server Spec'

os_name = attribute('os_name', default: '', description: 'The full name of the Windows OS')
os_version = attribute('os_version', default: '', description: 'The Version of the Windows OS')
tcp_ports = attribute('tcp_ports', default: [], description: 'The allowed list of listening TCP ports')
udp_ports = attribute('udp_ports', default: [], description: 'The allowed list of listening UDP ports')
features = attribute('features', default: [], description: 'The allowed list of installed windows features')
products = attribute('products', default: [], description: 'The allowed list of apps')
ignore_uninstalled_features = attribute('ignore_uninstalled_features', default: false, description: 'Ignore failures if all the features listed are NOT installed')
ignore_uninstalled_products = attribute('ignore_uninstalled_products', default: false, description: 'Ignore failures if all the products listed are NOT installed')

# Clean name to match new standard defined by train in fix (https://github.com/chef/train/issues/191)
# This can be removed when using inspec >= 1.49.2
formatted_os_name = os[:name].to_s
formatted_os_name.downcase!.tr!(' ', '_') if formatted_os_name =~ /[A-Z ]/
# # #
control 'csg-windows-1' do
  title 'CSG-Windows-1- Verify OS Version'
  desc "Verify the OS Version is #{os_name}"
  impact 1.0 # This is critical
  # Clean name to match new standard defined by train in fix (https://github.com/chef/train/issues/191)
  # This can be removed when using inspec >= 1.49.2
  os_name.downcase!.tr!(' ', '_') if os_name =~ /[A-Z ]/
  # # #
  describe formatted_os_name do
    it { should eq os_name }
  end
  unless os_version.to_s.empty?
    describe os[:release] do
      it { should eq os_version }
    end
  end
end

# # no users with admin in name
# control 'csg-windows-2' do
#   title 'CSG-Windows-2 Verify No Users with "admin" in the name'
#   desc 'Verify No Users with "admin" in the name'
#   impact 1.0 # This is critical
#   describe users.where { username.downcase.include? 'admin' } do
#     it { should_not exist }
#   end
# end

if formatted_os_name.include?('_server_') && !os[:release].to_s.start_with?('6.0')  # don't do features if windows 7 or windows 10 or 2008 server (not r2)
  # features
  control 'csg-windows-7.1' do
    title 'CSG-Windows-7.1 Verify unspecifed features are NOT installed'
    desc 'Verify unspecifed features are NOT installed.'
    impact 1.0 # This is critical
    feature_nots = windows_feature_product_helper.installed_features.reject { |i| features.include? i }
    describe windows_feature_product_helper do
      feature_nots.each do |f|
        its('installed_features') { should_not contain_one f }
      end
    end
  end

  control 'csg-windows-7.5' do
    title 'CSG-Windows-7.5 Verify features command did not contain errors'
    desc 'Verify features command did not contain errors.'
    impact 1.0 # This is critical
    describe windows_feature_product_helper do
      its('features_stderr') { should eq '' }
      its('features_exit_status') { should eq 0 }
    end
  end

  unless ignore_uninstalled_features
    control 'csg-windows-7.2' do
      title 'CSG-Windows-7.2 Verify all specifed features are installed'
      desc 'Verify all specifed features are, in fact, installed.'
      impact 0.5 # This is critical
      describe windows_feature_product_helper do
        features.each do |f|
          its('installed_features') { should contain_one f }
        end
      end
    end
  end
end

# products
control 'csg-windows-7.3' do
  title 'CSG-Windows-7.3 Verify unspecifed products are NOT installed'
  desc 'Verify unspecifed products are NOT installed.'
  impact 1.0 # This is critical
  product_nots = windows_feature_product_helper.installed_products.reject { |i| products.include? i }
  describe windows_feature_product_helper do
    product_nots.each do |f|
      its('installed_products') { should_not contain_one f }
    end
  end
end

control 'csg-windows-7.6' do
  title 'CSG-Windows-7.6 Verify products commands did not contain errors'
  desc 'Verify products commands did not contain errors.'
  impact 1.0 # This is critical
  describe windows_feature_product_helper do
    its('products_stderr') { should eq '' }
    its('products_wow_stderr') { should eq '' }
    its('products_exit_status') { should eq 0 }
    its('products_wow_exit_status') { should eq 0 }
  end
end

unless ignore_uninstalled_products
  control 'csg-windows-7.4' do
    title 'CSG-Windows-7.4 Verify specifed products are installed'
    desc 'Verify specifed products are, in fact, installed.'
    impact 0.5 # This is critical
    describe windows_feature_product_helper do
      products.each do |f|
        its('installed_products') { should contain_one f }
      end
    end
  end
end

# control 'csg-windows-9' do
#   impact 1.0
#   title 'CSG-Windows-9 Review password policy'
#   desc 'Set Enforce password history to 8 or more passwords, max password age is 90 and min password age is 1'
#   describe security_policy do
#     its('PasswordHistorySize') { should be >= 4 }
#     its('MaximumPasswordAge') { should be <= 90 }
#     its('MaximumPasswordAge') { should be > 0 }
#     its('MinimumPasswordAge') { should be >= 1 }
#   end
# end
#
# control 'csg-windows-10' do
#   impact 1.0
#   title 'CSG-Windows-9 Review account lockout policy'
#   desc 'Set Enforce account lockout policy of 4'
#   describe security_policy do
#     its('LockoutBadCount') { should be <= 5 }
#     its('ResetLockoutCount') { should_not eq 0 }
#     its('LockoutDuration') { should_not eq 0 }
#   end
# end

# ports
control 'csg-windows-12' do
  title 'CSG-Windows-12 Verify only specifed ports are listening'
  desc 'Verify only specifed ports are listening.'
  impact 1.0 # This is critical

  tcp_nots = windows_port_helper.listening_tcp_ports.reject { |p| tcp_ports.include? p[:port] }
  udp_nots = windows_port_helper.listening_udp_ports.reject { |p| udp_ports.include? p[:port] }

  describe windows_port_helper do
    tcp_nots.each do |p|
      its('listening_tcp_ports') { should_not contain_port p }
    end
    udp_nots.each do |p|
      its('listening_udp_ports') { should_not contain_port p }
    end
  end
end
