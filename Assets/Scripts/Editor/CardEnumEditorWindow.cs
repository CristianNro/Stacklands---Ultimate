using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CardEnumEditorWindow : EditorWindow
{
    private const string EnumFileRelativePath = "Assets/Scripts/CardData/Data/CardEnums.cs";

    private readonly List<EnumInfo> enums = new();
    private int selectedEnumIndex;
    private int selectedMemberIndex = -1;
    private string newMemberName = string.Empty;
    private string renameMemberName = string.Empty;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Stacklands/Card Enum Editor")]
    public static void Open()
    {
        CardEnumEditorWindow window = GetWindow<CardEnumEditorWindow>("Card Enum Editor");
        window.minSize = new Vector2(520f, 420f);
        window.ReloadEnums();
    }

    private void OnEnable()
    {
        ReloadEnums();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Card Enums Source", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(EnumFileRelativePath, EditorStyles.helpBox);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Reload"))
                ReloadEnums();

            GUI.enabled = File.Exists(GetAbsoluteEnumFilePath());
            if (GUILayout.Button("Open Source File"))
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(EnumFileRelativePath));
            GUI.enabled = true;
        }

        if (enums.Count == 0)
        {
            EditorGUILayout.HelpBox("No enums were found in CardEnums.cs.", MessageType.Warning);
            return;
        }

        EditorGUI.BeginChangeCheck();
        selectedEnumIndex = EditorGUILayout.Popup("Enum", selectedEnumIndex, GetEnumNames());
        if (EditorGUI.EndChangeCheck())
        {
            selectedMemberIndex = -1;
            renameMemberName = string.Empty;
        }

        EnumInfo selectedEnum = GetSelectedEnum();
        if (selectedEnum == null)
            return;

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Members", EditorStyles.boldLabel);

        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(180f)))
        {
            scrollPosition = scroll.scrollPosition;

            for (int i = 0; i < selectedEnum.members.Count; i++)
            {
                string member = selectedEnum.members[i];

                using (new EditorGUILayout.HorizontalScope())
                {
                    bool isSelected = selectedMemberIndex == i;
                    if (GUILayout.Toggle(isSelected, member, "Button"))
                    {
                        if (selectedMemberIndex != i)
                            renameMemberName = member;

                        selectedMemberIndex = i;
                    }

                    GUI.enabled = !string.Equals(member, "None", StringComparison.Ordinal);
                    if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                        TryRemoveSelectedMember(selectedEnum, member);
                    GUI.enabled = true;
                }
            }
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Add Member", EditorStyles.boldLabel);
        newMemberName = EditorGUILayout.TextField("New Name", newMemberName);

        if (GUILayout.Button("Add Member"))
            TryAddMember(selectedEnum);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Rename Selected Member", EditorStyles.boldLabel);

        if (selectedMemberIndex < 0 || selectedMemberIndex >= selectedEnum.members.Count)
        {
            EditorGUILayout.HelpBox("Select a member from the list to rename it.", MessageType.Info);
        }
        else
        {
            string selectedMember = selectedEnum.members[selectedMemberIndex];
            EditorGUILayout.LabelField("Selected", selectedMember);
            renameMemberName = EditorGUILayout.TextField("Rename To", renameMemberName);

            GUI.enabled = !string.Equals(selectedMember, "None", StringComparison.Ordinal);
            if (GUILayout.Button("Rename Member"))
                TryRenameMember(selectedEnum, selectedMember);
            GUI.enabled = true;
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "This tool edits CardEnums.cs directly and refreshes Unity afterwards. " +
            "Use it for project enums only. Removing or renaming values can invalidate serialized inspector data.",
            MessageType.Warning);
    }

    private void ReloadEnums()
    {
        enums.Clear();

        string filePath = GetAbsoluteEnumFilePath();
        if (!File.Exists(filePath))
        {
            selectedEnumIndex = 0;
            selectedMemberIndex = -1;
            renameMemberName = string.Empty;
            return;
        }

        string source = File.ReadAllText(filePath);
        MatchCollection matches = Regex.Matches(
            source,
            @"public\s+enum\s+(?<name>\w+)\s*\{(?<body>[\s\S]*?)\}",
            RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            EnumInfo info = new EnumInfo
            {
                name = match.Groups["name"].Value,
                body = match.Groups["body"].Value
            };

            string[] lines = info.body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
                    continue;

                Match memberMatch = Regex.Match(trimmed, @"^(?<member>[A-Za-z_][A-Za-z0-9_]*)");
                if (!memberMatch.Success)
                    continue;

                info.members.Add(memberMatch.Groups["member"].Value);
            }

            enums.Add(info);
        }

        if (selectedEnumIndex >= enums.Count)
            selectedEnumIndex = 0;

        selectedMemberIndex = -1;
        renameMemberName = string.Empty;
    }

    private void TryAddMember(EnumInfo selectedEnum)
    {
        string candidate = newMemberName.Trim();
        if (!ValidateMemberName(candidate, out string error))
        {
            EditorUtility.DisplayDialog("Invalid Member", error, "OK");
            return;
        }

        if (selectedEnum.members.Contains(candidate))
        {
            EditorUtility.DisplayDialog("Duplicate Member", $"'{candidate}' already exists in {selectedEnum.name}.", "OK");
            return;
        }

        if (!TryUpdateEnumSource(selectedEnum.name, body => AddMemberToBody(body, candidate), out error))
        {
            EditorUtility.DisplayDialog("Add Failed", error, "OK");
            return;
        }

        newMemberName = string.Empty;
        ReloadEnums();
    }

    private void TryRemoveSelectedMember(EnumInfo selectedEnum, string member)
    {
        if (!EditorUtility.DisplayDialog(
                "Remove Enum Member",
                $"Remove '{member}' from {selectedEnum.name}?\n\nThis can break serialized inspector values that use this enum.",
                "Remove",
                "Cancel"))
        {
            return;
        }

        if (!TryUpdateEnumSource(selectedEnum.name, body => RemoveMemberFromBody(body, member), out string error))
        {
            EditorUtility.DisplayDialog("Remove Failed", error, "OK");
            return;
        }

        ReloadEnums();
    }

    private void TryRenameMember(EnumInfo selectedEnum, string currentName)
    {
        string candidate = renameMemberName.Trim();
        if (!ValidateMemberName(candidate, out string error))
        {
            EditorUtility.DisplayDialog("Invalid Member", error, "OK");
            return;
        }

        if (string.Equals(candidate, currentName, StringComparison.Ordinal))
        {
            EditorUtility.DisplayDialog("No Change", "The new name is equal to the current one.", "OK");
            return;
        }

        if (selectedEnum.members.Contains(candidate))
        {
            EditorUtility.DisplayDialog("Duplicate Member", $"'{candidate}' already exists in {selectedEnum.name}.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Rename Enum Member",
                $"Rename '{currentName}' to '{candidate}' in {selectedEnum.name}?\n\nThis can break serialized inspector values that use this enum.",
                "Rename",
                "Cancel"))
        {
            return;
        }

        if (!TryUpdateEnumSource(selectedEnum.name, body => RenameMemberInBody(body, currentName, candidate), out error))
        {
            EditorUtility.DisplayDialog("Rename Failed", error, "OK");
            return;
        }

        ReloadEnums();
    }

    private bool TryUpdateEnumSource(string enumName, Func<string, string> bodyUpdater, out string error)
    {
        string filePath = GetAbsoluteEnumFilePath();
        if (!File.Exists(filePath))
        {
            error = $"Enum source file was not found at {EnumFileRelativePath}.";
            return false;
        }

        string source = File.ReadAllText(filePath);
        Match enumMatch = Regex.Match(
            source,
            $@"public\s+enum\s+{Regex.Escape(enumName)}\s*\{{(?<body>[\s\S]*?)\}}",
            RegexOptions.Multiline);

        if (!enumMatch.Success)
        {
            error = $"Enum '{enumName}' was not found in {EnumFileRelativePath}.";
            return false;
        }

        string originalBody = enumMatch.Groups["body"].Value;
        string updatedBody = bodyUpdater(originalBody);

        if (originalBody == updatedBody)
        {
            error = $"No source changes were produced for enum '{enumName}'.";
            return false;
        }

        int bodyIndex = enumMatch.Groups["body"].Index;
        int bodyLength = enumMatch.Groups["body"].Length;
        string updatedSource = source.Remove(bodyIndex, bodyLength).Insert(bodyIndex, updatedBody);

        File.WriteAllText(filePath, updatedSource);
        AssetDatabase.Refresh();

        error = null;
        return true;
    }

    private static string AddMemberToBody(string body, string memberName)
    {
        string lineEnding = body.Contains("\r\n") ? "\r\n" : "\n";
        string indent = DetectMemberIndent(body);
        string[] lines = body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        int insertLineIndex = lines.Length;
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                insertLineIndex = i + 1;
                break;
            }
        }

        int lastMemberLineIndex = FindLastMemberLineIndex(lines);
        if (lastMemberLineIndex >= 0)
            lines[lastMemberLineIndex] = EnsureLineEndsWithComma(lines[lastMemberLineIndex]);

        List<string> updatedLines = new List<string>(lines);
        updatedLines.Insert(insertLineIndex, indent + memberName);
        return string.Join(lineEnding, updatedLines);
    }

    private static string RemoveMemberFromBody(string body, string memberName)
    {
        string lineEnding = body.Contains("\r\n") ? "\r\n" : "\n";
        string[] lines = body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        List<string> updatedLines = new List<string>(lines.Length);

        for (int i = 0; i < lines.Length; i++)
        {
            if (IsMemberLine(lines[i], memberName))
                continue;

            updatedLines.Add(lines[i]);
        }

        return string.Join(lineEnding, updatedLines);
    }

    private static string RenameMemberInBody(string body, string currentName, string newName)
    {
        string lineEnding = body.Contains("\r\n") ? "\r\n" : "\n";
        string[] lines = body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        Regex memberRegex = new Regex($@"\b{Regex.Escape(currentName)}\b");

        for (int i = 0; i < lines.Length; i++)
        {
            if (!IsMemberLine(lines[i], currentName))
                continue;

            lines[i] = memberRegex.Replace(lines[i], newName, 1);

            break;
        }

        return string.Join(lineEnding, lines);
    }

    private static string DetectMemberIndent(string body)
    {
        string[] lines = body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            Match match = Regex.Match(lines[i], @"^(?<indent>\s*)(?<member>[A-Za-z_][A-Za-z0-9_]*)");
            if (match.Success)
                return match.Groups["indent"].Value;
        }

        return "        ";
    }

    private static int FindLastMemberLineIndex(string[] lines)
    {
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
                continue;

            if (Regex.IsMatch(trimmed, @"^[A-Za-z_][A-Za-z0-9_]*"))
                return i;
        }

        return -1;
    }

    private static string EnsureLineEndsWithComma(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return line;

        int commentIndex = line.IndexOf("//", StringComparison.Ordinal);
        if (commentIndex >= 0)
        {
            string beforeComment = line.Substring(0, commentIndex).TrimEnd();
            if (!beforeComment.EndsWith(","))
                beforeComment += ",";

            return beforeComment + " " + line.Substring(commentIndex).TrimStart();
        }

        return line.TrimEnd().EndsWith(",") ? line : line.TrimEnd() + ",";
    }

    private static bool IsMemberLine(string line, string memberName)
    {
        return Regex.IsMatch(line, $@"^\s*{Regex.Escape(memberName)}\b");
    }

    private static bool ValidateMemberName(string candidate, out string error)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            error = "The enum member name is empty.";
            return false;
        }

        if (!Regex.IsMatch(candidate, @"^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            error = "Use a valid C# identifier: letters, digits and underscore, without spaces.";
            return false;
        }

        error = null;
        return true;
    }

    private string[] GetEnumNames()
    {
        string[] names = new string[enums.Count];
        for (int i = 0; i < enums.Count; i++)
            names[i] = enums[i].name;

        return names;
    }

    private EnumInfo GetSelectedEnum()
    {
        if (selectedEnumIndex < 0 || selectedEnumIndex >= enums.Count)
            return null;

        return enums[selectedEnumIndex];
    }

    private static string GetAbsoluteEnumFilePath()
    {
        if (string.IsNullOrWhiteSpace(Application.dataPath))
            return string.Empty;

        string normalizedRelative = EnumFileRelativePath.Replace("Assets/", string.Empty).Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(Application.dataPath, normalizedRelative);
    }

    [Serializable]
    private class EnumInfo
    {
        public string name;
        public string body;
        public List<string> members = new();
    }
}
