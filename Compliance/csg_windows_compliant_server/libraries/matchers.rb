# `contain_one` matcher
# You can use it in the following cases:
# - check if an item or array is included in a given array
# eg:
# describe windows_feature_product_helper do
#   its('installed_features') { should contain_one 'NET45' }
#   its('installed_products') { should contain_one 'chefclient' }
# end
RSpec::Matchers.define :contain_one do |package|
  match do |list|
    list.include?(package)
  end

  match_when_negated do |list|
    !list.include?(package)
  end

  failure_message do |list|
    "expected `#{package}` to be in the list."
  end

  failure_message_when_negated do |list|
    "expected `#{package}` not to be in the list."
  end
end

RSpec::Matchers.define :contain_port do |port|
  match do |list|
    list.include?(port)
  end

  match_when_negated do |list|
    !list.include?(port)
  end

  failure_message do |list|
    "expected port `#{port[:port]}` to be in the list. (process: #{port[:process]})"
  end

  failure_message_when_negated do |list|
    "expected port `#{port[:port]}` not to be in the list. (process: #{port[:process]})"
  end
end
