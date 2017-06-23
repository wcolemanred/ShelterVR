using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFDC.wef.ui
{
	public class ButtonAudio : MonoBehaviour
	{
		public AudioClip buttonClick;
		public AudioClip buttonSelect;

		public void playClicked()
		{
			playAudioClip(buttonClick, 0);
		}

		public void playSelected()
		{
			playAudioClip(buttonSelect, 24f);
		}

		private void playAudioClip(AudioClip clip, float gain)
		{
			GvrAudioSource source = GetComponentInParent<GvrAudioSource>();
			source.gainDb = gain;
			source.clip = clip;
			source.Play();
		}
	}
}
