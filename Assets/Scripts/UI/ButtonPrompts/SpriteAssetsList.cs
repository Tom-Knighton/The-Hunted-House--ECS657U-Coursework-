using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "List of Sprite Assets",
    menuName = "List of Sprite Assets", order = 0)]

public class SpriteAssetsList : ScriptableObject
{
    public List<TMP_SpriteAsset> SpriteAssets;
}
