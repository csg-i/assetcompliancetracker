class PciJsonParseError < StandardError
  def initialize(json, e)
    super "PCI Run Error Occurred parsing the ACT json string #{json} with message #{e.message}"
  end
end
