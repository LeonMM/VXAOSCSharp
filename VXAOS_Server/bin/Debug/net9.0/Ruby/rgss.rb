IGNORE_CONSTANTS = [
    'RESOLUTIONS',
    'LOADING_TIME',
    'LOADING_TITLES',
    'TITLE_BAR_HEIGHT',
    'HOTKEYS',
    'BALLOONS_KEYS',
    'SHOP_WEBSITE',
    'HOST',
    'PORT'
]
IGNORE_KEYWORDS = [
    'Font.',
    'FONT',
    '_ICON',
    'ICON_',
    '_KEY'
]

def pascal_case(str)
  str.split('_').map(&:capitalize).join
end

def normalize_string(str)
  return str if str.encoding == Encoding::UTF_8

  str.force_encoding('UTF-8')

  unless str.valid_encoding?
    str.encode(
      'UTF-8',
      invalid: :replace,
      undef: :replace,
      replace: ''
    )
  else
    str
  end
end

def normalize(obj)
  case obj
  when String
    normalize_string(obj)
  when Symbol
    obj.to_s
  when Array
    obj.map { |v| normalize(v) }
  when Hash
    obj.each_with_object({}) {|(k, v), h|
      key = k.is_a?(Symbol) ? pascal_case(k.to_s) : k
      h[key] = normalize(v)
    }
  else
    obj
  end
end
def export_module(mod)
  result = {}
  mod.constants.each { |const|
    name = pascal_case(const.to_s)
    next if IGNORE_CONSTANTS.include?(name)
    value = mod.const_get(const)
    result[name] = normalize(value)
  }
  result
end
def evaluate_source(source)
    filtered_source = source.lines.reject { |line|
        IGNORE_KEYWORDS.any? { |kw| line.include?(kw) }
    }.join
    eval(filtered_source)
end
