using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScannerController : MonoBehaviour{

	const int MAXDISTORSION = 76;
	const int MINDISTORSION = 1;
	const float MAXDISTANCE = 15;
	const float MINDISTANCE = 0.1f;

    public Transform origin;
	public Material scannerMat;
	public float initialScanDistance = 0;
	public float maxScanDistance = 20;
	public float scanThickness;
	public GridGenerator gridGenerator;
	
	[ColorUsage(true, true)]
	public List<Color> colors;

    Camera cam;
    float scanDistance;
    float scanDistance1;
    float scanDistance2;

	IEnumerator scanHandler = null;

	LensDistortion lensDistortion = null;
	DepthOfField dof = null;

	void Start(){
		PostProcessVolume ppv = gameObject.GetComponent<PostProcessVolume>();
		ppv.profile.TryGetSettings(out lensDistortion);
		ppv.profile.TryGetSettings(out dof);
	}

	void OnEnable(){
		cam = GetComponent<Camera>();
		cam.depthTextureMode = DepthTextureMode.Depth;
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst){
		scannerMat.SetVector("_WorldSpaceScannerPos", origin.position);
		scannerMat.SetFloat("_ScanDistance1", scanDistance1);
		scannerMat.SetFloat("_ScanDistance2", scanDistance2);
		scannerMat.SetFloat("_ScanWidth", scanThickness);

		if(colors.Count >= 3){
			scannerMat.SetColor("_FirstColor", colors[0]);
			scannerMat.SetColor("_SecondColor", colors[1]);
			scannerMat.SetColor("_FillColor", colors[2]);
		}

		raycastCornerBlit(src, dst, scannerMat);
	}

	void raycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat){
		float camFar = cam.farClipPlane;
		float camFov = cam.fieldOfView;
		float camAspect = cam.aspect;

		float fovWHalf = camFov * 0.5f;

		Vector3 toRight = cam.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
		Vector3 toTop = cam.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

		Vector3 topLeft = (cam.transform.forward - toRight + toTop);
		float camScale = topLeft.magnitude * camFar;

		topLeft.Normalize();
		topLeft *= camScale;

		Vector3 topRight = (cam.transform.forward + toRight + toTop);
		topRight.Normalize();
		topRight *= camScale;

		Vector3 bottomRight = (cam.transform.forward + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= camScale;

		Vector3 bottomLeft = (cam.transform.forward - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= camScale;

		RenderTexture.active = dest;

		mat.SetTexture("_MainTex", source);

		GL.PushMatrix();
		GL.LoadOrtho();

		mat.SetPass(0);

		GL.Begin(GL.QUADS);

		GL.MultiTexCoord2(0, 0.0f, 0.0f);
		GL.MultiTexCoord(1, bottomLeft);
		GL.Vertex3(0.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 0.0f);
		GL.MultiTexCoord(1, bottomRight);
		GL.Vertex3(1.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 1.0f);
		GL.MultiTexCoord(1, topRight);
		GL.Vertex3(1.0f, 1.0f, 0.0f);

		GL.MultiTexCoord2(0, 0.0f, 1.0f);
		GL.MultiTexCoord(1, topLeft);
		GL.Vertex3(0.0f, 1.0f, 0.0f);

		GL.End();
		GL.PopMatrix();
	}

    void Update(){
        if(Input.GetKeyUp(KeyCode.Space)){
            if(scanDistance <= 0){
                scan();
            }else{
                scanBack();
            }
        }
    }

	void checkAndBlock(){
		if(scanHandler != null){
			StopCoroutine(scanHandler);
		}
	}

    void scan(){
		checkAndBlock();
		scanHandler = scanCoroutine();
        StartCoroutine(scanHandler);
    }

    void scanBack(){
		checkAndBlock();
		scanHandler = scanBackCoroutine();
        StartCoroutine(scanHandler);
    }

    IEnumerator scanCoroutine(){
		distorsion();
        scanDistance = initialScanDistance;
		bool startShow = false;   
        while(scanDistance <= maxScanDistance ){
			if(scanDistance > maxScanDistance/2 && !startShow){
				gridGenerator.show();
				startShow = true;
			}
            scanDistance1 = scanDistance+1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance2 = scanDistance1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance2 = scanDistance1+1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance ++;

        }
        scanDistance = maxScanDistance;  
		scanHandler = null;
    }

	IEnumerator scanBackCoroutine(){
		bool startHide = false;   
        while(scanDistance > 0 ){
			if(scanDistance < maxScanDistance/2 && !startHide){
				gridGenerator.hide();
				startHide = true;
				distorsion();
			}
            scanDistance1 = scanDistance-1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance2 = scanDistance1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance2 = scanDistance1-1;
            yield return new WaitForSecondsRealtime(.01f);
            scanDistance --;

        }
        scanDistance = initialScanDistance; 
		scanHandler = null; 
    }

	void distorsion(){
		StartCoroutine(distorsionCoroutine());
	}

	IEnumerator distorsionCoroutine(){
		if(dof != null && lensDistortion != null){
			int distAmount = MAXDISTORSION;
			float distance = MINDISTANCE;

			while(distAmount >= MINDISTORSION ){
				lensDistortion.intensity.value = distAmount;
				dof.focusDistance.value = distance;
				distAmount -=5 ;
				distance += 0.1f;
				yield return new WaitForSecondsRealtime(.01f);
			}

			lensDistortion.intensity.value = MINDISTORSION;
			dof.focusDistance.value = MAXDISTANCE;
		}
    }
}
