class LinuxPackageHelper < Inspec.resource(1)
  name 'linux_package_helper'
  desc 'Contains Helper info to get all installed packages'
  example "
    linux_package_helper.installed_packages.each do |pkg|
      ...
    end

    linux_package_helper do
      its('installed_packages') { should contain_one 'package_name' }
    end

  "

  def initialize()
    return skip_resource 'The `linux_package_helper` resource is not supported on your OS.' unless inspec.os.redhat?
  end
  @@cache = nil

  def installed_packages(_provider = nil, _version = nil)
    info[:installed_packages]
  end

  def info
    if @@cache.nil?
      @@cache = {
        installed_packages: inspec.command("rpm -qa --queryformat '%{NAME}\\n'").stdout.split("\n")
      }
    end
    @@cache
  end

  def to_s
    'Linux Installed Package Helper'
  end
end
