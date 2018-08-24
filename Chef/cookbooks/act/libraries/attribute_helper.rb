module Act
  module Core
    module AttributeHelper
      def node_attr(*names)
        raise 'node object is not present' if node.nil?

        # extract and remove last name
        last_name = names.pop

        # start at the top
        hash = node

        # traverse each root and branch names
        names.each do |name|
          raise "name chain passed to node_attr contains a nil/empty name - chain: #{names}" if name.to_s.empty?
          hash = hash[name]
          return nil if hash.nil?
        end

        # return leaf value
        hash[last_name]
      end
    end
  end
end

Chef::Recipe.include(Act::Core::AttributeHelper)
Chef::Resource.include(Act::Core::AttributeHelper)
