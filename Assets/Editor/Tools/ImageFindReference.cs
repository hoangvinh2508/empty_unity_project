using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.UI;

public class ImageFindReference : ScriptableWizard
{
	[MenuItem ("DevTools/Find Image Reference")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<ImageFindReference>("Setting Scale Particle", "Apply", "Choose Image File");
	}
	
	private string guid = string.Empty;
	public string spritePath = string.Empty;
	public List<Sprite> listImage = new List<Sprite>();
	Dictionary<Image, string> spritePaths = new Dictionary<Image, string>();
	
	public string folderPath = string.Empty;
	public List<string> listPrefab = new List<string>();
	private List<FileInfo> listFile = new List<FileInfo> ();
	
	void OnWizardUpdate() {
		helpString = "Query throught all project and find prefab have image reference";
		
	}
	
	
	//	void SettingScale () {		
	//		foreach (var file in listFile) {			
	//			var content = string.Empty;
	//			using (StreamReader sr = file.OpenText()) {
	//				content = sr.ReadToEnd ();
	//				sr.Close ();
	//			}
	//	
	//			if (content.Contains ("scalingMode:")) {
	//				content = Regex.Replace (content, "scalingMode: 2", "scalingMode: 0");
	//				content = Regex.Replace (content, "scalingMode: 1", "scalingMode: 0");
	//			} else {
	//				content = Regex.Replace (content, "InitialModule:", "scalingMode: 0\n  InitialModule:");
	//			}
	//
	//			if (!content.Contains ("serializedVersion: 2\n  lengthInSec:")) {
	//				content = Regex.Replace (content, "lengthInSec:", "serializedVersion: 2\n  lengthInSec:");
	//			}
	//
	//			using (StreamWriter sw = file.CreateText ()) {
	//				sw.Write (content);
	//				sw.Close ();
	//			}
	//		}			
	//	}
	
	void ReadImageFile () {
		if (spritePath.Length > 0) {
			//FileInfo file = new FileInfo (spritePath);
			var lines = File.ReadAllLines(spritePath);
			
			foreach (var line in lines)
			{
				if (line.StartsWith("guid: ")) {
					guid = line.Remove(0, 6);
					break;
				}
			}
		}
	}
	
	void FindReferenceImage (DirectoryInfo dic) {
		FileInfo[] listFile = dic.GetFiles ();
		foreach (var file in listFile) {			
			if (file.Extension == ".prefab" || file.Extension == ".mat") {
				
				var content = string.Empty;
				using (StreamReader sr = file.OpenText()) {
					content = sr.ReadToEnd ();
					sr.Close ();
				}
				
				bool b = content.Contains (guid);
				if (b == true) {	
					this.listFile.Add (file);
					if (file.Extension == ".prefab")
						listPrefab.Add (file.Name.Remove(file.Name.Length - 7, 7));
					if (file.Extension == ".mat")
						listPrefab.Add (file.Name);
				}
			}
		}
		
		DirectoryInfo[] listDirectory = dic.GetDirectories ();
		foreach (var folder in listDirectory)
			FindReferenceImage (folder);
	}
	
	void OnWizardCreate () {
		//SettingScale ();
	}
	
	void OnWizardOtherButton ()
	{
		if (spritePath == string.Empty) {
			var pathFile = EditorUtility.OpenFilePanel ("Select file image", "", "");
			spritePath = pathFile;
			
		} else {
			FileInfo file = new FileInfo(spritePath);
			var pathFile = EditorUtility.OpenFilePanel ("Select file image", file.DirectoryName, "");
			spritePath = pathFile;
		}
		
		if (!spritePath.EndsWith (".meta")) {
			spritePath = spritePath + ".meta";
		}
		ReadImageFile ();

		if (folderPath == string.Empty) {
			var pathFolder = EditorUtility.OpenFolderPanel ("Select root folder", "", "");
			folderPath = pathFolder;
		}

		listFile.Clear ();
		listPrefab.Clear ();
		DirectoryInfo directory = Directory.CreateDirectory (folderPath);
		FindReferenceImage (directory);
		
	}
}

