using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableObject : Collectable
{
	private enum ECollectableVariant
	{
		CPU = 0,
		RAM,
		DISK
	}
	[SerializeField]
	private ECollectableVariant eCollectableVariant;
	[SerializeField]
	private ResourcesIndicators resourcesIndicators;
	public int amount = 40;
	override protected void OnCollect(GameObject target)
	{
		var ap = GetComponent<AudioPlayer>();
		if (ap)
		{
			ap.Audio_Play_Clip();
		}

		if (resourcesIndicators)
		{
			if (eCollectableVariant == ECollectableVariant.CPU)
			{
				resourcesIndicators.DoAdd("cpu", amount.ToString());
			}
			else if (eCollectableVariant == ECollectableVariant.RAM)
			{
				resourcesIndicators.DoAdd("ram", amount.ToString());
			}
			else if (eCollectableVariant == ECollectableVariant.DISK)
			{
				resourcesIndicators.DoAdd("disk", amount.ToString());
			}
		}
	}
}
