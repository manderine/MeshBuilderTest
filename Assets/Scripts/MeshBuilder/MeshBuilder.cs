//#define NGUI_USED

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MeshBuilder : MonoBehaviour {
#if NGUI_USED
	public string mSpriteName = "";
	public UIAtlas mAtlas = null;
#else
	public Material _Material = null;
	public Rect _SpriteRect = new Rect( 0, 0, 1, 1 );
#endif

	public Vector2 _Size = Vector2.one;

	MeshRenderer __meshRenderer = null;
	public MeshRenderer _meshRenderer {
		get { return __meshRenderer; }
	}

#if NGUI_USED
	public bool _isGrayScale {
		get { 
			MeshRenderer mr = _meshRenderer;
			if( (mr == null) || (mAtlas == null) ) {
				return false;
			}

			if( mr.sharedMaterial == mAtlas.spriteGrayScale ) {
				return true;
			}

			return false;
		}
		set {
			MeshRenderer mr = _meshRenderer;
			if( (mr == null) || (mAtlas == null) ) {
				return;
			}

			if( value == true ) {
				mr.sharedMaterial = mAtlas.spriteGrayScale;
			} else {
				mr.sharedMaterial = mAtlas.spriteMaterial;
			}
		}
	}
#else
	// Not Used
	public bool _isGrayScale {
		get; set;
	}
#endif

	private void Awake() {
		__meshRenderer = gameObject.GetComponent<MeshRenderer>();
	}

	Vector3 GetYPosition( Vector2 vpos, bool is_y ) {
		if( is_y == true ) {
			return new Vector3( vpos.x, vpos.y, 0 );
		}
		return new Vector3( vpos.x, 0, vpos.y );
	}

	Vector2 GetUVPosition( Vector3 vpos, Vector4 vsize, Vector4 usize, bool is_y ) {
		float xp = (vpos.x - vsize.x) / vsize.w;
		if( is_y == true ) {
			float yp = (vpos.y - vsize.y) / vsize.z;
			return new Vector2( usize.x + xp * usize.w, usize.y + yp * usize.z );
		}
		float zp = (vpos.z - vsize.y) / vsize.z;
		return new Vector2( usize.x + xp * usize.w, usize.y + zp * usize.z );
	}

	public void BuildMesh( bool is_y ) {
		Material mat = null;
		Texture tex = null;

		int paddingLeft = 0, paddingRight = 0, paddingTop = 0, paddingBottom = 0;
		int spr_x = 0, spr_y = 0, spr_w = 1, spr_h = 1;

#if NGUI_USED
		UISpriteData sprData = null;
		if( (mAtlas == null) || ((mat = mAtlas.spriteMaterial) == null) || ((tex = mat.mainTexture) == null) || ((sprData = mAtlas.GetSprite( mSpriteName )) == null) ) {
			Debug.Log( "Build error - Atlas:" + mAtlas + ", Material:" + mat + ", Texture:" + tex + ", SpriteData:" + sprData );
			return;
		}

		paddingLeft = sprData.paddingLeft;
		paddingRight = sprData.paddingRight;
		paddingTop = sprData.paddingTop;
		paddingBottom = sprData.paddingBottom;

		spr_x = sprData.x;
		spr_y = sprData.y;
		spr_w = sprData.width;
		spr_h = sprData.height;
#else
		if( ((mat = _Material) == null) || ((tex = mat.mainTexture) == null) ) {
			Debug.Log( "Build error - Material:" + mat + ", Texture:" + tex );
			return;
		}

		spr_x = Mathf.RoundToInt( _SpriteRect.x );
		spr_y = Mathf.RoundToInt( _SpriteRect.y );
		spr_w = Mathf.RoundToInt( _SpriteRect.width );
		spr_h = Mathf.RoundToInt( _SpriteRect.height );
#endif

		int width = paddingLeft + spr_w + paddingRight;
		int height = paddingTop + spr_h + paddingBottom;

		int numbers = 1;
		int numVertices = numbers * 4;
		int numTriangles = numbers * 6;

		float left = 0;
		float top = height;
		float right = width;
		float bottom = 0;

		left += paddingLeft;
		right -= paddingRight;
		top -= paddingTop;
		bottom += paddingBottom;

		Vector4 vsize = Vector4.zero;
		vsize.x = ((float)left / width) * _Size.x - _Size.x * 0.5f;
		vsize.y = ((float)bottom / height) * _Size.y - _Size.y * 0.5f;
		vsize.w = ((float)right / width) * _Size.x - _Size.x * 0.5f;
		vsize.z = ((float)top / height) * _Size.y - _Size.y * 0.5f;

		vsize.w -= vsize.x; // w
		vsize.z -= vsize.y; // h

		Vector2 [] _vertices = new Vector2 [ 4 ];

		_vertices[0] = new Vector2( vsize.x, vsize.y );
		_vertices[1] = new Vector2( vsize.x + vsize.w, vsize.y);
		_vertices[2] = new Vector2( vsize.x + vsize.w, vsize.y + vsize.z );
		_vertices[3] = new Vector2( vsize.x, vsize.y + vsize.z );

		Vector4 usize = Vector4.zero;

		usize.x = (float)spr_x / tex.width;
		usize.y = (tex.height - ((float)spr_y + spr_h)) / tex.height;
		usize.w = ((float)spr_x + spr_w) / tex.width;
		usize.z = (tex.height - (float)spr_y) / tex.height;

		usize.w -= usize.x; // w
		usize.z -= usize.y; // h

		// Generate the mesh data
		Vector3 [] vertices = new Vector3[ numVertices ];
		Vector3 [] normals = new Vector3[ numVertices ];
		Vector2 [] uv = new Vector2[ numVertices ];
		int [] triangles = new int [ numTriangles ];

		int ori_index = 0;
		Vector2 start = _vertices[ ori_index++ ];
		Vector2 second = _vertices[ ori_index++ ];
		Vector2 third, fourth;

		for( int i=0; i<numbers; i++ ) {
			int vertexIndex = i * 4;
			int triangleIndex = i * 6;

			third = _vertices[ ori_index++ ];
			fourth = _vertices[ ori_index++ ];

			vertices[ vertexIndex + 0 ] = GetYPosition( start, is_y );
			vertices[ vertexIndex + 1 ] = GetYPosition( second, is_y );
			vertices[ vertexIndex + 2 ] = GetYPosition( third, is_y );
			vertices[ vertexIndex + 3 ] = GetYPosition( fourth, is_y );

			second = fourth;

			uv[ vertexIndex + 0 ] = GetUVPosition( vertices[ vertexIndex + 0 ], vsize, usize, is_y );
			uv[ vertexIndex + 1 ] = GetUVPosition( vertices[ vertexIndex + 1 ], vsize, usize, is_y );
			uv[ vertexIndex + 2 ] = GetUVPosition( vertices[ vertexIndex + 2 ], vsize, usize, is_y );
			uv[ vertexIndex + 3 ] = GetUVPosition( vertices[ vertexIndex + 3 ], vsize, usize, is_y );

			normals[ vertexIndex + 0 ] = new Vector3( 0, 0, -1 );
			normals[ vertexIndex + 1 ] = new Vector3( 0, 0, -1 );
			normals[ vertexIndex + 2 ] = new Vector3( 0, 0, -1 );
			normals[ vertexIndex + 3 ] = new Vector3( 0, 0, -1 );

			triangles[ triangleIndex + 0 ] = vertexIndex;
			triangles[ triangleIndex + 1 ] = vertexIndex + 1;
			triangles[ triangleIndex + 2 ] = vertexIndex + 2;

			triangles[ triangleIndex + 3 ] = vertexIndex;
			triangles[ triangleIndex + 4 ] = vertexIndex + 2;
			triangles[ triangleIndex + 5 ] = vertexIndex + 3;
		}

		// Create a new Mesh and populate with the data
		Mesh mesh = new Mesh();

		mesh.name = "tile_map";
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.triangles = triangles;

		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if( mf == null ) {
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mf.sharedMesh = mesh;
		
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		if( mr == null ) {
			mr = gameObject.AddComponent<MeshRenderer>();
		}
		mr.sharedMaterial = mat;

		mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows = false;

		__meshRenderer = mr;

		Debug.Log( "Done Mesh!" );
	}

	public void BuildDestroy() {
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		if( mr != null ) {
			DestroyImmediate( mr );
		}

		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		if( mf != null ) {
			DestroyImmediate( mf );
		}

		__meshRenderer = null;
	}
}
