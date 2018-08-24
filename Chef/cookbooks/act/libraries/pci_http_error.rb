class PciHttpError < StandardError
  def initialize(url, query, e)
    super "PCI Run Error Occurred making an ACT http call to #{url}#{query} with message #{e.message}"
  end
end
