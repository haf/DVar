require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/release'
require 'albacore/tasks/versionizer'
require 'albacore/task_types/nugets_pack'
require './tools/paket_pack'
include ::Albacore::NugetsPack

Configuration = ENV['CONFIGURATION'] || 'Release'

Albacore::Tasks::Versionizer.new :versioning

desc 'create assembly infos'
asmver_files :assembly_info do |a|
  a.files = FileList['./src/DVar/*.fsproj']
  a.attributes assembly_description: 'A functional way to configure functions',
               assembly_configuration: Configuration,
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_compile do |b|
  b.prop 'Configuration', Configuration
  b.sln = 'src/DVar/DVar.fsproj'
end

task :paket_bootstrap do
  system 'tools/paket.bootstrapper.exe', clr_command: true unless   File.exists? 'tools/paket.exe'
end

desc 'restore all nugets as per the packages.config files'
task :restore => :paket_bootstrap do
  system 'tools/paket.exe', 'restore', clr_command: true
end

desc 'Perform full build'
task :compile => [:versioning, :restore, :assembly_info, :quick_compile]

directory 'build/pkg'


task :create_nugets_quick do
  projects = FileList['src/**/*.fsproj']
  knowns = Set.new(projects.map { |f| Albacore::Project.new f }.map { |p| p.id })
  authors = "https://twitter.com/eulerfx"
  projects.each do |f|
    p = Albacore::Project.new f
    n = create_nuspec p, knowns
    d = get_dependencies n
    m = %{type file
id #{p.id}
version #{ENV['NUGET_VERSION']}
title #{p.id}
authors #{authors}
owners #{authors}
tags dvar configuration config
description A functional way of configuring functions
language en-GB
copyright #{authors}
licenseUrl https://www.apache.org/licenses/LICENSE-2.0.html
projectUrl https://github.com/haf/dvar
iconUrl https://raw.githubusercontent.com/haf/dvar/master/tools/logo.png
files
  #{p.proj_path_base}/#{p.output_dll Configuration} ==\> lib/net461
releaseNotes
  #{n.metadata.release_notes.each_line.reject{|x| x.strip == ""}.join}
dependencies
  #{d}
}
    begin
      File.open("paket.template", "w") do |template|
        template.write m
      end
      system "tools/paket.exe", %w|pack output build/pkg|, clr_command: true
    ensure
      File.delete "paket.template"
    end
  end
end

desc 'Build nuget packages'
task :create_nugets => [:versioning, 'build/pkg', :compile, :create_nugets_quick]

namespace :tests do
  #task :unit do
  #  system "src/MyProj.Tests/bin/#{Configuration}/MyProj.Tests.exe", clr_command: true
  #end
end

# task :tests => :'tests:unit'

task :default => :create_nugets #, :tests ]

task :ensure_nuget_key do
  raise 'missing env NUGET_KEY value' unless ENV['NUGET_KEY']
end

Albacore::Tasks::Release.new :release,
                             pkg_dir: 'build/pkg',
                             depend_on: [:create_nugets, :ensure_nuget_key],
                             nuget_exe: 'packages/NuGet.CommandLine/tools/NuGet.exe',
                             api_key: ENV['NUGET_KEY']
