default['act'].tap do |act|
  act['url'].tap do |url|
    url['protocol'] = 'https'
    url['port'] = 443
    url['server'] = 'act.myserver.org'
    url['path'] = '/BuildSpec/RetrieveFor'
    url['put_path'] = '/BuildSpec/AssignTo'
    url['fqdn_arg'] = 'fqdn'
  end
  act['inspec_download_url_prefix'] = 'https://artifacts.myorg.org/act/inspec/'
end
override['audit'].tap do |audit|
  audit['reporter'] = 'chef-server-automate'
  audit['quiet'] = true
  audit['insecure'] = false
  audit['inspec_version'] = '1.27.0' if Chef::VERSION.start_with?('12.')
  audit['inspec_version'] = '2.2.55' unless Chef::VERSION.start_with?('12.')
  audit['profiles'] = []
end
