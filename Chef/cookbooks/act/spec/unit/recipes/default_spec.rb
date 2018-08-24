#
# Cookbook Name:: csg_win_core
# Spec:: default
#
# Copyright (c) 2017 The Authors, All Rights Reserved.

require 'spec_helper'

%w(
  act::default
).each do |recipe|
  describe recipe do
    context 'When all attributes are default, on windows 2012R2' do
      let(:chef_run) do
        ChefSpec::ServerRunner.new(platform: 'windows', version: '2012R2').converge(described_recipe)
      end
      it 'converges successfully' do
        expect { chef_run }.to_not raise_error
      end
    end
  end
end
