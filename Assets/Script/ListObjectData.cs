using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Scriptable Objects/Object")]
public class ListObjectData : ScriptableObject
{
    List<HitObjectData> hitObjectDatas = new List<HitObjectData>();
}
