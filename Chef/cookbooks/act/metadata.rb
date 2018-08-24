name 'act'
maintainer 'Jay Brummels'
description 'Recipe to call ACT compliance url and download the spec for the node based on FQDN and then run the approproate inspec tests via the Audit cookbook'
long_description 'Recipe to call ACT compliance url and download the spec for the node based on FQDN and then run the approproate inspec tests via the Audit cookbook'
version '1.0'
issues_url 'https://github.com/csg-i/assetcompliancetracker' if respond_to?(:issues_url)
source_url 'https://github.com/csg-i/assetcompliancetracker' if respond_to?(:source_url)
chef_version '>= 12.1'
supports 'windows'
supports 'redhat'
supports 'centos'

depends 'audit', '~> 7.0.1'
