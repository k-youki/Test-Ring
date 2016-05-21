using UnityEngine;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;

// info.plistの構成
//
//<?xml version="1.0" encoding="UTF-8"?>
//<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
//<plist version="1.0">
//  <dict>
//    <key>...</key>
//    <string>...</string>
//    ...
//  </dict>
//</plist>

public class PlistMod {
	// すべての設定の直接の親であるdictエレメントを取得
	private static XmlNode FindPlistDictNode(XmlDocument doc) {
		var cur = doc.FirstChild;
		while (cur != null) {
			if (cur.Name.Equals ("plist") && cur.ChildNodes.Count == 1) {
				var dict = cur.FirstChild;
				if (dict.Name.Equals ("dict")) {
					return dict;
				}
			}
			cur = cur.NextSibling;
		}
		return null;
	}
	
	// すでにそのkeyが存在しているか？
	// dict:親ノード
	private static bool HasKey(XmlNode dict, string keyName) {
		var cur = dict.FirstChild;
		while (cur != null) {
			if (cur.Name.Equals ("key") && cur.InnerText.Equals (keyName)) {
				return true;
			}
			cur = cur.NextSibling;
		}
		return false;
	}
	
	// 子エレメントを追加
	// elementName:<...>の<>の中の文字列
	// innerText:<key>...</key>のタグで囲まれた文字列
	private static XmlElement AddChildElement(XmlDocument doc, XmlNode parent,
	                                          string elementName, string innerText = null) {
		var newElement = doc.CreateElement (elementName);
		if (!string.IsNullOrEmpty (innerText)) {
			newElement.InnerText = innerText;
		}
		parent.AppendChild (newElement);
		return newElement;
	}
	
	// 指定したkeyに対応する値を更新する
	// <key>KEY_TEXT</key>
	// <ELEMENT_NAME>VALUE</ELEMENT_NAME>
	// 以上の構造の場合のみ正常に動作
	// key:KEY_TEXT
	// elementName:ELEMENT_NAME
	// value:VALUE
	private static XmlNode UpdateKeyValue(XmlNode node, string key, string elementName, string value){
		// まず<key>...</key>のノードを取得
		var keyNode = GetChildElement (node, "key", key);
		if (keyNode.NextSibling != null && keyNode.NextSibling.Name.Equals (elementName)) {
			// 取得したkeyノードの次のノードのelementNameが指定された文字列だった場合、値を更新する
			keyNode.NextSibling.InnerText = value;
			return keyNode;
		}
		return null;
	}
	
	// 子エレメントを取得
	// elementName:<...>の<>の中の文字列
	// innerText:<key>...</key>のタグで囲まれた文字列
	private static XmlNode GetChildElement(XmlNode node, string elementName, string innerText=null) {
		var cur = node.FirstChild;
		while (cur != null) {
			if (cur.Name.Equals (elementName)) {
				if ((innerText == null && cur.InnerText == null) ||
				    (innerText != null && cur.InnerText.Equals (innerText))) {
					return cur;
				}
			}
			cur = cur.NextSibling;
		}
		return null;
	}
	
	// info.plistのあるディレクトリパスと設定値を受け取り、info.plistに設定を登録する
	public static void UpdatePlist(string path) {
		Debug.Log("UpdatePlist");
		// info.plistを読み込む
		string fullPath = Path.Combine (path, "info.plist");
		var doc = new XmlDocument();
		doc.Load (fullPath);
		
		// すべての設定の直接の親であるdictエレメントを取得する
		var dict = FindPlistDictNode (doc);
		if (dict == null) {
			Debug.LogError ("Error plistの解析に失敗　パス:" + fullPath);
			return;
		}

//		<key>NSAppTransportSecurity</key>
//			<dict>
//				<key>NSExceptionDomains</key>
//					<dict>
//						<key>localhost</key>
//						<dict>
//							<key>NSTemporaryExceptionAllowsInsecureHTTPLoads</key>
//							<true/>
//						</dict>
//					</dict>
//			</dict>
		{
			XmlNode ATSSetting = null;
			if (!HasKey (dict, "NSAppTransportSecurity")) {
				AddChildElement (doc, dict, "key", "NSAppTransportSecurity");
				ATSSetting = AddChildElement (doc, dict, "dict");
				Debug.Log("None NSAppTransportSecurity");
			} else {
				//すでにkey:App Transport Security Settingが存在している
				//key:App Transport Security Settingを取得
				var ats = GetChildElement (dict, "key", "NSAppTransportSecurity");
//				XmlNode atsGC = ats.NextSibling;
				ATSSetting = ats.NextSibling;
				Debug.Log("OK NSAppTransportSecurity");
			}
			//存在確認・更新
			bool isExist = false;
			foreach (XmlNode atsDict in ATSSetting.ChildNodes) {
				if (atsDict.Name.Equals ("dict") && atsDict.HasChildNodes) {
					//子がdict構造であり、更に子を持っている
					var exDict = GetChildElement (atsDict, "key", "NSExceptionDomains");
					if (exDict.Name.Equals ("dict") && exDict.HasChildNodes) {
						var lhDict = GetChildElement (exDict, "key", "localhost");
						if (lhDict.Name.Equals ("dict") && lhDict.HasChildNodes) {
							var atsBool = GetChildElement (lhDict, "key", "localhost");
							if (atsBool.Name.Equals ("boolean") && atsBool.Value.Equals("true") ) {
							
								isExist = true;
								atsBool.Value = @"ture";

								break;
							}
						}
					}
				}
			}
			if (!isExist) {
				//存在していない場合のみ追加
				Debug.Log("isExist = false");
				AddChildElement (doc, ATSSetting, "key", "NSExceptionDomains");//AddChildElement (doc, ATSSetting, "dict");
				var lhDict = AddChildElement (doc, ATSSetting, "dict");
				AddChildElement (doc, lhDict, "key", "localhost");
				var httplDict = AddChildElement (doc, lhDict, "dict");
				AddChildElement (doc, httplDict, "key", "NSTemporaryExceptionAllowsInsecureHTTPLoads");
				AddChildElement (doc, httplDict, "true");
			}
		}


		// 保存
		doc.Save(fullPath);
		
		// <!DOCTYPE　の行を書き換えて保存してしまうため、修正する
		string textPlist = string.Empty;
		using (var reader = new StreamReader (fullPath)) {
			textPlist = reader.ReadToEnd ();
		}
		
		// 本来の行が存在していれば処理終了
		int fixupStart = textPlist.IndexOf ("<!DOCTYPE plist PUBLIC", System.StringComparison.Ordinal);
		if (fixupStart <= 0) {
			return;
		}
		int fixupEnd = textPlist.IndexOf ('>', fixupStart);
		if (fixupEnd <= 0) {
			return;
		}
		
		// 修正処理
		string fixedPlist = textPlist.Substring (0, fixupStart);
		fixedPlist += "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">";
		fixedPlist += textPlist.Substring (fixupEnd+1);
		
		using (var writer = new StreamWriter (fullPath, false)) {
			writer.Write (fixedPlist);
		}
	}

	// 関数OnPostprocessBuildを追加。
	// Assets/Editorフォルダに、このスクリプトをいれておくと自動でInfo.plistを書き換える。
	// UIInterfaceOrientationは次の４つの値をとりうる。
	//   UIInterfaceOrientationPortrait
	//   UIInterfaceOrientationPortraitUpsideDown
	//   UIInterfaceOrientationLandscapeLeft
	//   UIInterfaceOrientationLandscapeRight
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		UpdatePlist(pathToBuiltProject);
	}
}