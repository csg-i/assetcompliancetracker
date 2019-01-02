
resource_name :assign_to_act_buildspec
provides :assign_to_act_buildspec
property :app_spec_id, String, name_property: true

action_class do
  def whyrun_supported?
    true
  end
end

action :default do
  new_resource.ignore_failure = true # always ignore failure
  protocol = new_resource.node_attr('act', 'url', 'protocol')
  server = new_resource.node_attr('act', 'url', 'server')
  port = new_resource.node_attr('act', 'url', 'port')
  path = new_resource.node_attr('act', 'url', 'put_path')
  fqdn_arg = new_resource.node_attr('act', 'url', 'fqdn_arg')
  fqdn = new_resource.node_attr('fqdn')
  url = "#{protocol}://#{server}:#{port}"
  query = "#{path}/#{new_resource.app_spec_id}?#{fqdn_arg}=#{fqdn}"
  converge_by('Assign ACT Application Build Specification') do
    log "using url #{url} and query #{query}" do
      level :debug
    end
    begin
      response = Chef::HTTP.new(url).put(query, '{}')
      log 'Assign ACT Application Build Specification Response' do
        message response
        level :debug
      end
    rescue Timeout::Error, Errno::EINVAL, Errno::ECONNRESET, EOFError, Net::HTTPBadResponse, Net::HTTPHeaderSyntaxError, Net::ProtocolError => e
      Chef::Log.fatal "CHEF HTTP Error #{e.to_s}"
      raise PciHttpError.new url, query, e
    end
  end
end
