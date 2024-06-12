using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(anchorTool))]
class anchorToolEditor : Editor
{
	void OnSceneGUI()
	{
		if (Event.current.type == EventType.MouseUp)
		{
			anchorTool myTarget = (anchorTool)target;
			myTarget.StopDrag();
		}
	}
}