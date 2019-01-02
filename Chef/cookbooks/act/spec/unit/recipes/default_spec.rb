#
# Cookbook Name:: csg_win_core
# Spec:: default
#
# Copyright (c) 2017 The Authors, All Rights Reserved.

require 'spec_helper'

%w(
  act::assign_act_application_spec_sample
  act::set_audit_attributes
).each do |recipe|
  describe recipe do
    context 'When all attributes are default, on windows 2012R2' do
      let(:chef_run) do
        ChefSpec::ServerRunner.new(step_into:['assign_to_act_buildspec', 'set_act_buildspec_attributes'], platform: 'windows', version: '2012R2').converge(described_recipe)
      end

      before :each do
        # mock the REST Call
        allow_any_instance_of(Chef::HTTP).to receive(:put).with(any_args).and_call_original
        allow_any_instance_of(Chef::HTTP).to receive(:put).with('/BuildSpec/AssignTo/1200?fqdn=fauxhai.local', '{}').and_return('{}')
        allow_any_instance_of(Chef::HTTP).to receive(:get).with(any_args).and_call_original
        allow_any_instance_of(Chef::HTTP).to receive(:get).with('/BuildSpec/RetrieveFor?fqdn=fauxhai.local').and_return('{"fqdn":"fauxhai.local","os_name":"Unknown","os_version":null,"tcp_ports":[],"udp_ports":[],"tcp6_ports":[],"udp6_ports":[],"features":[],"products":[],"packages":[]}')
      end
      it 'converges successfully' do
        expect { chef_run }.to_not raise_error
      end
    end
  end
end
