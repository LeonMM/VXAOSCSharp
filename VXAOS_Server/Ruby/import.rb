require 'json'
require 'base64'
require_relative 'rpg'
$stdout.sync = true
$stderr.sync = true
Encoding.default_external = Encoding::UTF_8
Encoding.default_internal = Encoding::UTF_8

def load_data(file_name)
	File.open("#{$data_path}/#{file_name}", 'rb') { |f| Marshal.load(f) }
end

def load_game_data
    $data_actors        = load_data("Actors.rvdata2")
    $data_classes       = load_data("Classes.rvdata2")
    $data_skills        = load_data("Skills.rvdata2")
    $data_items         = load_data("Items.rvdata2")
    $data_weapons       = load_data("Weapons.rvdata2")
    $data_armors        = load_data("Armors.rvdata2")
    $data_enemies       = load_data("Enemies.rvdata2")
    $data_troops        = load_data("Troops.rvdata2")
    $data_states        = load_data("States.rvdata2")
    $data_tilesets      = load_data("Tilesets.rvdata2")
    $data_common_events = load_data("CommonEvents.rvdata2")
    $data_system        = load_data("System.rvdata2")
    $data_mapinfos      = load_data("MapInfos.rvdata2")
end


def export_jsons
    $log = ""
    puts $data_actors.to_json
    puts $data_classes.to_json
    puts $data_skills.to_json
    puts $data_items.to_json
    puts $data_weapons.to_json
    puts $data_armors.to_json
    puts $data_enemies.to_json
    puts $data_troops.to_json
    puts $data_states.to_json
    puts $data_tilesets.to_json
    puts $data_common_events.to_json
    puts $data_system.to_json
    puts $data_mapinfos.to_json
    $data_mapinfos.keys.each{|map_id|
        map = load_data(sprintf('Map%03d.rvdata2', map_id))
        puts map.to_json
    }
    File.open(File.join(__dir__, "import.txt"), "w") do |file|
      file.puts $log
    end
end


# Method to check if a directory exists
def directory_exists?(directory)
    File.directory?(directory)
end

# Method to check if required files exist in the directory
def required_files_exist?(directory, required_files)
    required_files.all? { |file| File.file?(File.join(directory, file)) }
end

# Array of required files
required_files = [
    'Actors.rvdata2', 'Classes.rvdata2', 'Skills.rvdata2', 'Items.rvdata2',
    'Weapons.rvdata2', 'Armors.rvdata2', 'Enemies.rvdata2', 'Troops.rvdata2',
    'States.rvdata2', 'Tilesets.rvdata2', 'CommonEvents.rvdata2',
    'System.rvdata2', 'MapInfos.rvdata2'
]
      
data = Base64.decode64(ARGV[0])
directory_path = data.force_encoding('UTF-8')

if directory_exists?(directory_path)
    if required_files_exist?(directory_path, required_files)
        $data_path = directory_path
        load_game_data
        export_jsons
    else
        puts 'error1'
    end
    else
    puts 'error2'
end