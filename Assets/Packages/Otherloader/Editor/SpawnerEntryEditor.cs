#if H3VR_IMPORTED
using FistVR;
using OtherLoader;
#endif

using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSpawnerEntry), true)]
public class SpawnerEntryEditor : Editor
{
#if H3VR_IMPORTED
    
    private enum AmmoCategoryType
    {
        None, Magazine, Clip, Speedloader, Cartridge
    }
    
    private bool _hasInit;
    private bool _isItemIDEmpty;
    private ItemSpawnerV2.PageMode? _previousPage;
    private AmmoCategoryType _ammoCategory = AmmoCategoryType.None;
    
    public override void OnInspectorGUI()
    {
        serializedObject.ApplyModifiedProperties();

        var entry = serializedObject.targetObject as ItemSpawnerEntry;

        if (!_hasInit)
        {
            _isItemIDEmpty = string.IsNullOrEmpty(entry.MainObjectID);
            
            try
            {
                var parts = entry.EntryPath.Split('/');
                _ammoCategory = (AmmoCategoryType)Enum.Parse(typeof(AmmoCategoryType), parts[1]);
            }
            catch (Exception)
            {
                // ignored
            }

            _hasInit = true;
        }

        var property = serializedObject.GetIterator();
        if (property == null || !property.NextVisible(true))
            return;
        
        var page = (ItemSpawnerV2.PageMode)serializedObject.FindProperty("Page").enumValueIndex;
        var subCatProperty = serializedObject.FindProperty("SubCategory");
        var subCat = (ItemSpawnerID.ESubCategory)subCatProperty.enumValueIndex;
        var isAmmoPage = page == ItemSpawnerV2.PageMode.Ammo;
        var hasPageChanged = _previousPage != null && _previousPage != page;

        do
        {
            if (property.name == "MainObjectID")
            {
                if (entry.MainObjectObj != null)
                {
                    property.stringValue = entry.MainObjectObj.ItemID;
                }
            }

            if (property.name == "MainObjectObj" || property.name == "EntryIcon" || property.name == "UsesLargeSpawnPad")
            {
                DrawHorizontalLine();
            }

            if (property.name == "SubCategory")
            {
                if (isAmmoPage)
                {
                    _ammoCategory = (AmmoCategoryType)EditorGUILayout.EnumPopup(
                        "Ammo Category",
                        _ammoCategory
                    );
                }
                else
                    DrawProperty(property);

                continue;
            }

            if (property.name == "EntryPath")
            {
                DrawHorizontalLine();
                
                var values = property.stringValue.Split('/').ToList();
                property.stringValue = page.ToString();
                property.stringValue += "/";

                // Ammo page doesn't use subcategories
                if (isAmmoPage)
                {
                    if (hasPageChanged)
                        _ammoCategory = AmmoCategoryType.None;
                    
                    subCatProperty.enumValueIndex = (int)ItemSpawnerID.ESubCategory.None;

                    if (_ammoCategory != AmmoCategoryType.None)
                        property.stringValue += _ammoCategory.ToString();
                    else if (!hasPageChanged)
                        property.stringValue += values[1];
                }
                else
                {
                    if (hasPageChanged)
                        subCatProperty.enumValueIndex = (int)ItemSpawnerID.ESubCategory.None;
                    
                    _ammoCategory = AmmoCategoryType.None;
                    
                    if (subCat != ItemSpawnerID.ESubCategory.None)
                        property.stringValue += subCat.ToString();
                    else if (!hasPageChanged)
                        property.stringValue += values[1];
                }

                // Add ObjectID at the end of the path
                var itemID = serializedObject.FindProperty("MainObjectID").stringValue;
                if (!string.IsNullOrEmpty(itemID))
                {
                    //If the itemID field is currently filled, but previously wasn't, we fill maintain all of the path and then add the itemID
                    if (_isItemIDEmpty)
                    {
                        for (int i = 2; i < values.Count; i++)
                        {
                            property.stringValue += "/" + values[i];
                        }

                        _isItemIDEmpty = false;
                    }


                    //If the itemID field was already filled previously, we can just draw everything until the itemID, and then add the itemID
                    else
                    {
                        for (int i = 2; i < values.Count - 1; i++)
                        {
                            property.stringValue += "/" + values[i];
                        }
                    }

                    property.stringValue += "/" + serializedObject.FindProperty("MainObjectID").stringValue;
                }

                else
                {
                    _isItemIDEmpty = true;

                    for (int i = 2; i < values.Count; i++)
                    {
                        property.stringValue += "/" + values[i];
                    }
                }
                
            }

            DrawProperty(property);
        }
        while (property.NextVisible(false));

        _previousPage = page;
    }


    protected virtual void DrawProperty(SerializedProperty property)
    {
        EditorGUILayout.PropertyField(property, true);
    }

    private void DrawHorizontalLine()
    {
        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

#endif

}

