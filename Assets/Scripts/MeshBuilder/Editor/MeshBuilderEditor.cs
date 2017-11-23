//#define NGUI_USED

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshBuilder))]
[CanEditMultipleObjects]
public class MeshBuilderEditor : Editor {
	public override void OnInspectorGUI() {
		EditorGUIUtility.labelWidth = 80f;
		EditorGUILayout.Space();

		serializedObject.Update();

		MeshBuilder mb = target as MeshBuilder;
		if( mb != null ) {
			ShouldDrawProperties( mb );

			GUILayout.BeginHorizontal();

			if( GUILayout.Button( "Build - Y" ) ) {
				BuildMesh( targets, true );
			}

			if( GUILayout.Button( "Build - Z" ) ) {
				BuildMesh( targets, false );
			}

			GUILayout.EndHorizontal();

			if( mb._meshRenderer != null ) {
				GUILayout.BeginHorizontal();

#if NGUI_USED
				if( mb._isGrayScale == false ) {
					if( GUILayout.Button( "Gray On" ) ) {
						BuildGray( targets, true );
					}
				} else {
					if( GUILayout.Button( "Gray Off" ) ) {
						BuildGray( targets, false );
					}
				}
#endif

				if( GUILayout.Button( "Build Destroy" ) ) {
					BuildDestroy( targets );
				}

				GUILayout.EndHorizontal();
			}
		}
		
		serializedObject.ApplyModifiedProperties();
	}

	protected void BuildMesh( Object [] objs, bool is_y ) {
		for( int i=0; i<objs.Length; i++ ) {
			(objs[i] as MeshBuilder).BuildMesh( is_y );
		}
	}

	protected void BuildGray( Object [] objs, bool is_gray ) {
		for( int i=0; i<objs.Length; i++ ) {
			(objs[i] as MeshBuilder)._isGrayScale = is_gray;
		}
	}

	protected void BuildDestroy( Object [] objs ) {
		for( int i=0; i<objs.Length; i++ ) {
			(objs[i] as MeshBuilder).BuildDestroy();
		}
	}

#if NGUI_USED
	protected void ShouldDrawProperties( MeshBuilder mb ) {
		GUILayout.BeginHorizontal();
		if( GUILayout.Button( "Atlas", "DropDown", GUILayout.Width(76f)) ) {
			ComponentSelector.Show<UIAtlas>( OnSelectAtlas );
		}
		SerializedProperty atlas = serializedObject.FindProperty( "mAtlas" );
		EditorGUILayout.PropertyField( atlas, new GUIContent(""), GUILayout.MinWidth(20f) );

		//mb.mAtlas = (UIAtlas)EditorGUILayout.ObjectField( "", mb.mAtlas, typeof(UIAtlas), true, GUILayout.MinWidth(20f) );
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if( GUILayout.Button( "Sprite", "DropDown", GUILayout.Width(76f)) ) {
			NGUISettings.atlas = mb.mAtlas;
			NGUISettings.selectedSprite = mb.mSpriteName;
			SpriteSelector.Show( SelectSprite );
		}
		GUILayout.Label( mb.mSpriteName, "HelpBox", GUILayout.Height(18f) );
		GUILayout.Space( 18f );
		GUILayout.EndHorizontal();

		SerializedProperty sp = serializedObject.FindProperty( "_Size" );
		EditorGUILayout.PropertyField( sp );
	}

	void OnSelectAtlas( Object obj ) {
		serializedObject.Update();

		SerializedProperty sp = serializedObject.FindProperty( "mAtlas" );
		sp.objectReferenceValue = obj;

		serializedObject.ApplyModifiedProperties();

		EditorUtility.SetDirty( serializedObject.targetObject );
	}

	/// <summary>
	/// Sprite selection callback function.
	/// </summary>

	void SelectSprite( string spriteName ) {
		serializedObject.Update();

		SerializedProperty sp = serializedObject.FindProperty( "mSpriteName" );
		sp.stringValue = spriteName;

		serializedObject.ApplyModifiedProperties();

		EditorUtility.SetDirty( serializedObject.targetObject );
	}
#else
	protected void ShouldDrawProperties( MeshBuilder mb ) {
		base.OnInspectorGUI();
	}
#endif

	public override bool HasPreviewGUI() {
		return (Selection.activeGameObject == null || Selection.gameObjects.Length == 1);
	}

	public override void OnPreviewGUI( Rect drawRect, GUIStyle background ) {
		MeshBuilder mb = target as MeshBuilder;
#if NGUI_USED
		if( (mb == null) || (mb.mAtlas == null) ) {
			return;
		}

		Texture2D tex = mb.mAtlas.texture as Texture2D;
		if( tex == null ) {
			return;
		}

		UISpriteData sprData = mb.mAtlas.GetSprite( mb.mSpriteName );
		if( sprData == null ) {
			return;
		}

		NGUIEditorTools.DrawSprite( tex, drawRect, sprData, Color.white );
#else
		Texture tex2D;
		if( (mb._Material == null) || ((tex2D = mb._Material.mainTexture) == null) ) {
			return;
		}

		Rect outerRect = drawRect;
		Rect sprRect = mb._SpriteRect;

		outerRect.width = sprRect.width;
		outerRect.height = sprRect.height;

		if( sprRect.width > 0 ) {
			float f = drawRect.width / outerRect.width;
			outerRect.width *= f;
			outerRect.height *= f;
		}

		if( drawRect.height > outerRect.height ) {
			outerRect.y += (drawRect.height - outerRect.height) * 0.5f;
		} else if( outerRect.height > drawRect.height ) {
			float f = drawRect.height / outerRect.height;
			outerRect.width *= f;
			outerRect.height *= f;
		}

		if( drawRect.width > outerRect.width ) {
			outerRect.x += (drawRect.width - outerRect.width) * 0.5f;
		}

		Rect uv = sprRect;
		int width = tex2D.width;
		int height = tex2D.height;

		if( width != 0f && height != 0f ) {
			uv.xMin = sprRect.xMin / width;
			uv.xMax = sprRect.xMax / width;
			uv.yMin = 1f - sprRect.yMax / height;
			uv.yMax = 1f - sprRect.yMin / height;
		}

		GUI.DrawTextureWithTexCoords( outerRect, tex2D, uv, true );
#endif
	}
}
