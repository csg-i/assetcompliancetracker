resource_name :assign_to_act_buildspec
provides :assign_to_act_buildspec
property :app_spec_id, Bignum, name_property: true

action_class do
  def whyrun_supported?
    true
  end
end

action :default do
  # Chef Compliance InSpec Profiles
  log "Using pci compliance via url #{compliance_url} for #{new_resource.name}"

  protocol = new_resource.node_attr('csg', 'pci', 'build_spec', 'url', 'protocol')
  server = new_resource.node_attr('csg', 'pci', 'build_spec', 'url', 'server')
  port = new_resource.node_attr('csg', 'pci', 'build_spec', 'url', 'port')
  path = new_resource.node_attr('csg', 'pci', 'build_spec', 'url', 'put_path')
  fqdn_arg = new_resource.node_attr('csg', 'pci', 'build_spec', 'url', 'fqdn_arg')
  fqdn = new_resource.node_attr('fqdn')
  url = "#{protocol}://#{server}:#{port}"
  query = "#{path}/#{app_spec_id}?#{fqdn_arg}=#{fqdn}"
  log "using url #{url} and query #{query}"

  begin
    response = Chef::HTTP.new(url).put(query)
    log 'Assign ACT Application Build Specification Response' do
      message response
      level :info
    end
  rescue Timeout::Error, Errno::EINVAL, Errno::ECONNRESET, EOFError, Net::HTTPBadResponse, NET::HTTPNotFound, Net::HTTPHeaderSyntaxError, Net::ProtocolError => e
    log 'CHEF HTTP Error' do
      message e.to_s
      level :fatal
    end
    raise PciHttpError.new url, query, e
  end
end
