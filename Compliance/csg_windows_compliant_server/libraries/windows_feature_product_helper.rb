class WindowsFeatureProductHelper < Inspec.resource(1)
  name 'windows_feature_product_helper'
  desc 'Contains Helper info to get all installed packages'
  example "
    windows_feature_product_helper.installed_products.each do |pkg|
      ...
    end
    OR
    windows_feature_product_helper.installed_features.each do |pkg|
      ...
    end
    OR
    describe windows_feature_product_helper do
      its('installed_features') { should_not contain_one 'Wow64-Support' }
      its('installed_products') { shouldt contain_one 'Chef Client 12.21.3' }
      its('features_stderr') { should eq '' }
      its('products_stderr') { should eq '' }
      its('products_wow_stderr') { should eq '' }
      its('features_exit_status') { should eq 0 }
      its('products_exit_status') { should eq 0 }
      its('products_wow_exit_status') { should eq 0 }
    end
  "

  def initialize()
    return skip_resource 'The `windows_feature_product_helper` resource is not supported on your OS.' unless inspec.os.windows?
  end
  @@cache = nil

  def installed_features(_provider = nil, _version = nil)
    info[:features][:features]
  end

  def features_stderr(_provider = nil, _version = nil)
    info[:features][:stderr]
  end

  def features_exit_status(_provider = nil, _version = nil)
    info[:features][:exit_status]
  end

  def installed_products(_provider = nil, _version = nil)
    info[:products][:products]
  end

  def products_stderr(_provider = nil, _version = nil)
    info[:products][:stderr]
  end

  def products_exit_status(_provider = nil, _version = nil)
    info[:products][:exit_status]
  end

  def products_wow_stderr(_provider = nil, _version = nil)
    info[:products][:wow_stderr]
  end

  def products_wow_exit_status(_provider = nil, _version = nil)
    info[:products][:wow_exit_status]
  end

  def info
    if @@cache.nil?
      products = query_installed_products
      wow64 = query_installed_products_wow64

      @@cache = {
        features: query_installed_features,
        products: {
          products: products[:products].concat(wow64[:products]).uniq,
          stderr: products[:stderr],
          exit_status: products[:exit_status],
          wow_stderr: wow64[:stderr],
          wow_exit_status:wow64[:exit_status]
        }
      }
    end
    return @@cache
  end

  def to_s
    'Windows Installed Feature/Package Helper'
  end

  private

  def query_installed_features
    installed_features_cmd = 'Get-WindowsFeature | ?{ $_.Installed -eq $true } | Select-Object -ExpandProperty Name | foreach {$_.Trim()} | ConvertTo-Json'
    cmd = inspec.command installed_features_cmd
    out = json_parse_safe_array cmd.stdout
    return {
      features: out,
      stderr: cmd.stderr,
      exit_status: cmd.exit_status
    }
  end

  def query_installed_products
    installed_products_cmd = 'Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | ?{$_.DisplayName -ne $null -and $_.DisplayName.Length -ne 0 } | select -ExpandProperty DisplayName | foreach {$_.Trim()} | Sort -Unique | ConvertTo-Json'
    cmd = inspec.command(installed_products_cmd)
    out = json_parse_safe_array cmd.stdout
    return {
      products: out,
      stderr: cmd.stderr,
      exit_status: cmd.exit_status
    }
  end

  def query_installed_products_wow64
    installed_products_cmd = <<-EOH
    if( Test-Path HKLM:\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall ) {
      Get-ItemProperty HKLM:\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\* | ?{$_.DisplayName -ne $null -and $_.DisplayName.Length -ne 0 } | select -ExpandProperty DisplayName | foreach {$_.Trim()} | Sort -Unique | ConvertTo-Json
    }
    EOH
    cmd = inspec.command(installed_products_cmd)
    out = json_parse_safe_array cmd.stdout
    return {
      products: out,
      stderr: cmd.stderr,
      exit_status: cmd.exit_status
    }
  end

  def json_parse_safe_array(str)
    begin
      arr = JSON.parse(str)
    rescue JSON::ParserError => _e
      return []
    end
    unless arr.class.to_s == 'Array'
      if arr.nil?
        arr = []
      else
        tmp = []
        tmp.push arr
        arr = tmp
      end
    end
    arr.uniq
  end
end
