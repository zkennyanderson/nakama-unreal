/*
 * Copyright 2019 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using UnrealBuildTool;

public class Nakama : ModuleRules
{
	private string m_libSuffix;

	public Nakama(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicIncludePaths.AddRange(
			new string[] {
				Path.Combine(ModulePath, "..", "ThirdParty")
			});

		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",
				//"Projects"
				// ... add other public dependencies that you statically link with here ...
			});

		string libsPath = LibsPath;
		
		switch (Target.Platform)
		{
			case UnrealTargetPlatform.Win32:
				libsPath = Path.Combine(libsPath, "win32");
				break;

			case UnrealTargetPlatform.Win64:
				libsPath = Path.Combine(libsPath, "win64");
				break;

			case UnrealTargetPlatform.Linux:
				libsPath = Path.Combine(libsPath, "linux");
				break;

			case UnrealTargetPlatform.Mac:
				libsPath = Path.Combine(libsPath, "mac");
				break;

			case UnrealTargetPlatform.IOS:
				libsPath = Path.Combine(libsPath, "ios");
				break;

			case UnrealTargetPlatform.Android:
				libsPath = Path.Combine(libsPath, "android");
				break;

			case UnrealTargetPlatform.PS4:
			case UnrealTargetPlatform.XboxOne:
			case UnrealTargetPlatform.HTML5:
			default:
				throw new NotImplementedException("Nakama Unreal client does not currently support platform: " + Target.Platform.ToString());
		}

		if (Target.Platform == UnrealTargetPlatform.Win32 || Target.Platform == UnrealTargetPlatform.Win64)
		{
			switch (Target.WindowsPlatform.Compiler)
			{
				#if !UE_4_22_OR_LATER
				case WindowsCompiler.VisualStudio2015: libsPath = Path.Combine(libsPath, "v140"); break;
				#endif
				case WindowsCompiler.VisualStudio2017: libsPath = Path.Combine(libsPath, "v141"); break;
				#if UE_4_22_OR_LATER
				case WindowsCompiler.VisualStudio2019: libsPath = Path.Combine(libsPath, "v142"); break;
				#endif
				default:
					throw new NotImplementedException("Nakama Unreal client does not currently support compiler: " + Target.WindowsPlatform.GetVisualStudioCompilerVersionName());
			}

			//if (Target.Configuration == UnrealTargetConfiguration.DebugGame || Target.Configuration == UnrealTargetConfiguration.DebugGameEditor)
			/*{
				libsPath = Path.Combine(libsPath, "Debug");
				m_libSuffix = "d";
			}
			else*/
			{
				libsPath = Path.Combine(libsPath, "Release");
				m_libSuffix = "";
			}
		}

		PublicLibraryPaths.Add(libsPath);

		if (Target.Platform == UnrealTargetPlatform.Win32 || Target.Platform == UnrealTargetPlatform.Win64)
		{
			PublicAdditionalLibraries.Add("nakama-cpp" + m_libSuffix + ".lib");
			CopyToBinaries(Path.Combine(libsPath, "nakama-cpp" + m_libSuffix + ".dll"), Target);
			PublicDelayLoadDLLs.AddRange(new string[] { "nakama-cpp" + m_libSuffix + ".dll" });
		}
		else if (Target.Platform == UnrealTargetPlatform.Mac || Target.Platform == UnrealTargetPlatform.IOS)
		{
			PublicAdditionalLibraries.Add(Path.Combine(libsPath, "libnakama-cpp.dylib"));
		}
		else
		{
			// XXX: For some reason, we have to add the full path to the .a file here or it is not found :(
			PublicAdditionalLibraries.Add(Path.Combine(libsPath, "libnakama-cpp.so"));
		}

		PrivateDefinitions.Add("NAKAMA_SHARED_LIBRARY=1");
	}

	private void CopyToBinaries(string Filepath, ReadOnlyTargetRules Target)
	{
		string binariesDir = Path.Combine(ProjectBinariesPath, Target.Platform.ToString());
		string filename = Path.GetFileName(Filepath);

		if (!Directory.Exists(binariesDir))
			Directory.CreateDirectory(binariesDir);

		//File.Copy(Filepath, Path.Combine(binariesDir, filename), false);
		CopyFile(Filepath, Path.Combine(binariesDir, filename));
	}

	private string ProjectBinariesPath
	{
		get
		{
			var basePath = Path.GetDirectoryName(RulesCompiler.GetFileNameFromType(GetType()));
			return Path.Combine(basePath, "..", "..", "..", "..", "Binaries");
			//return Path.Combine(
			//	  Directory.GetParent(ModulePath).Parent.Parent.ToString(), "Binaries");
		}
	}
	
	private void CopyFile(string source, string dest)
	{
		System.Console.WriteLine("Copying {0} to {1}", source, dest);
		if (System.IO.File.Exists(dest))
		{
			System.IO.File.SetAttributes(dest, System.IO.File.GetAttributes(dest) & ~System.IO.FileAttributes.ReadOnly);
		}
		try
		{
			System.IO.File.Copy(source, dest, true);
		}
		catch (System.Exception ex)
		{
			System.Console.WriteLine("Failed to copy file: {0}", ex.Message);
		}
	}

	private string ModulePath
	{
		get { return ModuleDirectory; }
	}

	private string LibsPath
	{
		//get { return Path.Combine(ModulePath, "Private", "libs"); }
		get { return Path.Combine(ModulePath, "Private", "shared-libs"); }
	}
}
