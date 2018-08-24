resource_name :set_act_buildspec_attributes
provides :set_act_buildspec_attributes

action_class do
  def whyrun_supported?
    true
  end

  def compliance_profile
    profile = 'linux' unless platform?('windows')
    profile = 'windows' if platform?('windows')
    "csg_#{profile}_compliant_server"
  end

  def compliance_url
    url = new_resource.node_attr('act', 'inspec_download_url_prefix')
    "#{url}#{compliance_profile}.tar.gz"
  end
end

action :default do
  # Chef Compliance InSpec Profiles
  log "Using ACT pci compliance via url #{compliance_url} for #{new_resource.name}"

  protocol = new_resource.node_attr('act', 'url', 'protocol')
  server = new_resource.node_attr('act', 'url', 'server')
  port = new_resource.node_attr('act', 'url', 'port')
  path = new_resource.node_attr('act', 'url', 'path')
  fqdn_arg = new_resource.node_attr('act', 'url', 'fqdn_arg')
  fqdn = new_resource.node_attr('fqdn')
  url = "#{protocol}://#{server}:#{port}"
  query = "#{path}?#{fqdn_arg}=#{fqdn}"

  log "using url #{url} and query #{query}"

  begin
    json = Chef::HTTP.new(url).get(query)
    log 'JSON ACT BuildSpec Arguments' do
      message json
      level :debug
    end
  rescue Timeout::Error, Errno::EINVAL, Errno::ECONNRESET, EOFError, Net::HTTPBadResponse, Net::HTTPHeaderSyntaxError, Net::ProtocolError => e
    log 'CHEF HTTP Error' do
      message e.to_s
      level :fatal
    end
    raise PciHttpError.new url, query, e
  end

  begin
    hash = JSON.parse(json)
  rescue JSON::ParserError => e
    log 'JSON Parse Error' do
      message e.to_s
      level :fatal
    end
    raise PciJsonParseError.new json, e
  end

  node.force_override['audit'].tap do |audit|
    audit['profiles'] = [
      {
        name: compliance_profile,
        url: compliance_url,
      },
    ]

    audit['attributes'].merge! hash
  end
end
