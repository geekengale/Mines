using System;
using UnityEngine;

public enum UIElements {  }

/// <summary>
/// UICollection Wrapper
/// </summary>
[Serializable]
public class UICollection : GenericEnumCollection<GameObject, UIElements> {}