using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    public string name;
    public GameObject prefab;
    public int stackMax;
    public string type;
    public bool isRange;
}
