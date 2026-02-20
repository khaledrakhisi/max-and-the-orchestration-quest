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
	private Container container;
	public int amount = 40;
	override protected void OnCollect(GameObject target)
	{
		var ap = GetComponent<AudioPlayer>();
		if (ap)
		{
			ap.Audio_Play_Clip();
		}

		if (container)
		{
			if (eCollectableVariant == ECollectableVariant.CPU)
			{
				container.DoAdd("cpu", amount.ToString());
			}
			else if (eCollectableVariant == ECollectableVariant.RAM)
			{
				container.DoAdd("ram", amount.ToString());
			}
			else if (eCollectableVariant == ECollectableVariant.DISK)
			{
				container.DoAdd("disk", amount.ToString());
			}
		}
	}
}
