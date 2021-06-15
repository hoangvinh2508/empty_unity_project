using UnityEngine;
using System.IO;
using System.Xml;

namespace UnityEditor.MonsterEditor
{
    public class PlistMod
    {
        private static XmlNode FindPlistDictNode(XmlDocument doc)
        {
            var curr = doc.FirstChild;
            while(curr != null)
            {
                if(curr.Name.Equals("plist") && curr.ChildNodes.Count == 1)
                {
                    var dict = curr.FirstChild;
                    if(dict.Name.Equals("dict"))
                        return dict;
                }
                curr = curr.NextSibling;
            }
            return null;
        }
        
        private static XmlElement AddChildElement(XmlDocument doc, XmlNode parent, string elementName, string innerText=null)
        {
            var newElement = doc.CreateElement(elementName);
            if(!string.IsNullOrEmpty(innerText))
                newElement.InnerText = innerText;
            
            parent.AppendChild(newElement);
            return newElement;
        }

        private static bool HasKey(XmlNode dict, string keyName)
        {
            var curr = dict.FirstChild;
            while(curr != null)
            {
                if(curr.Name.Equals("key") && curr.InnerText.Equals(keyName))
                    return true;
                curr = curr.NextSibling;
            }
            return false;
        }
        
		public static void UpdatePlist(string path, string bundleId, string googleReversedClientId)      {
            const string fileName = "Info.plist";
            string fullPath = Path.Combine(path, fileName);
            
			if(string.IsNullOrEmpty(googleReversedClientId) || string.IsNullOrEmpty(bundleId))
            {
                Debug.LogError("You didn't specify a Goole revertsed Client ID or bundleId.");
                return;
            }
            
            var doc = new XmlDocument();
            doc.Load(fullPath);

            var dict = FindPlistDictNode(doc);
            if(dict == null)
            {
                Debug.LogError("Error parsing " + fullPath);
                return;
            }

			/*
			<key>UIRequiresFullScreen</key>
			<true/>
			 */
			if(!HasKey(dict, "UIRequiresFullScreen"))
			{
				AddChildElement(doc, dict, "key", "UIRequiresFullScreen");
				XmlElement child = doc.CreateElement("true");
				dict.AppendChild(child);
			}

			/*
			 	<key>LSApplicationQueriesSchemes</key>
				<array>
				 <string>urlscheme</string>
				</array>
			 */
			if(!HasKey(dict, "LSApplicationQueriesSchemes"))
            {
				AddChildElement(doc, dict, "key", "LSApplicationQueriesSchemes");
				var innerArray = AddChildElement(doc, dict, "array");
                {
					AddChildElement(doc, innerArray, "string", "line");
					AddChildElement(doc, innerArray, "string", "lobi");
					AddChildElement(doc, innerArray, "string", "twitter");
                    AddChildElement(doc, innerArray, "string", "monsterdrive");
                }
            }
            
            //here's how the custom url scheme should end up looking
            /*
             <key>CFBundleURLTypes</key>
             <array>
                 <dict>
                     <key>CFBundleURLSchemes</key>
                     <array>
                         <string>fbYOUR_APP_ID</string>
                     </array>
                 </dict>
             </array>
            */
            if(!HasKey(dict, "CFBundleURLTypes"))
            {
                AddChildElement(doc, dict, "key", "CFBundleURLTypes");
                var urlSchemeTop = AddChildElement(doc, dict, "array");
                {
                    var urlSchemeBundleDict = AddChildElement(doc, urlSchemeTop, "dict");
                    {
						AddChildElement(doc, urlSchemeBundleDict, "key", "CFBundleURLSchemes");
						var innerArray = AddChildElement(doc, urlSchemeBundleDict, "array");
                        {
                            AddChildElement(doc, innerArray, "string", bundleId);
                        }
                    }

					var urlSchemeGoogleDict = AddChildElement(doc, urlSchemeTop, "dict");
					{
						AddChildElement(doc, urlSchemeGoogleDict, "key", "CFBundleURLSchemes");
						var innerArray = AddChildElement(doc, urlSchemeGoogleDict, "array");
						{
							AddChildElement(doc, innerArray, "string", googleReversedClientId);
						}
					}
                }
            }
            
            
            doc.Save(fullPath);
            
            //the xml writer barfs writing out part of the plist header.
            //so we replace the part that it wrote incorrectly here
            var reader = new StreamReader(fullPath);
            string textPlist = reader.ReadToEnd();
            reader.Close();
            
            int fixupStart = textPlist.IndexOf("<!DOCTYPE plist PUBLIC", System.StringComparison.Ordinal);
            if(fixupStart <= 0)
                return;
            int fixupEnd = textPlist.IndexOf('>', fixupStart);
            if(fixupEnd <= 0)
                return;
            
            string fixedPlist = textPlist.Substring(0, fixupStart);
            fixedPlist += "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">";
            fixedPlist += textPlist.Substring(fixupEnd+1);
            
            var writer = new StreamWriter(fullPath, false);
            writer.Write(fixedPlist);
            writer.Close();
        }
    }
}
