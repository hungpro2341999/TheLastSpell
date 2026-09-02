using TPLib;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Camera;

public class MaskableBehaviour : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("_maskMaterial")]
	protected Material maskMaterial;

	protected LutMaskGenerator maskGenerator;

	private Material backupMaterial;

	private Renderer myRenderer;

	private Coroutine coroutine;

	protected void Start()
	{
		Init();
	}

	protected void OnEnable()
	{
		if (base.isActiveAndEnabled)
		{
			Init();
		}
	}

	public void Init()
	{
		maskGenerator = TPSingleton<LutMaskGenerator>.Instance;
		if (maskGenerator == null)
		{
			TPDebug.LogError("Mask Generator not found !", this);
			base.enabled = false;
			return;
		}
		myRenderer = GetComponent<Renderer>();
		if (myRenderer == null)
		{
			TPDebug.LogError("No component with material found on this MaskableBehaviour", this);
		}
		else
		{
			backupMaterial = myRenderer.material;
		}
	}

	private void OnBecameInvisible()
	{
		if (base.enabled)
		{
			maskGenerator.MaskableObjects.Remove(this);
		}
	}

	private void OnBecameVisible()
	{
		if (base.enabled)
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
			}
			if (!maskGenerator.MaskableObjects.Contains(this))
			{
				maskGenerator.MaskableObjects.Add(this);
			}
		}
	}

	public void SwitchMaterial(bool useMaskMat = true)
	{
		myRenderer.material = (useMaskMat ? maskMaterial : backupMaterial);
	}
}
