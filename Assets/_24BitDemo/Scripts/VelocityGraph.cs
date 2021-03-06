﻿using System.Collections.Generic;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace Demo
{
	public class VelocityGraph : MonoBehaviour
	{
		protected enum Velocity
		{
			Movement,
			Forward,
			Vertical,
			Lateral
		}

		[SerializeField]
		protected Velocity direction;
		
		[SerializeField]
		protected int numberBars = 240;

		[SerializeField]
		protected float barWidthPercOfScreenWidth = 0.00125f;

		[SerializeField]
		protected float barScale = 10;

		Vector3 lastPosition;
		Vector3 deltaPosition;
		float speed;
		readonly List<float> deltas = new List<float>();
		Texture2D texture;

		void Awake()
		{
			texture = new Texture2D(1, 1);
		}

		void OnDestroy()
		{
			Destroy(texture);
		}

		void Update()
		{
			var currentPosition = transform.position;
			deltaPosition = currentPosition - lastPosition;
			switch (direction)
			{
				case Velocity.Movement:
					speed = deltaPosition.magnitude;
					break;
				case Velocity.Forward:
					speed = deltaPosition.GetMagnitudeOnAxis(transform.forward);
					break;
				case Velocity.Vertical:
					speed = deltaPosition.GetMagnitudeOnAxis(transform.up);
					break;
				case Velocity.Lateral:
					speed = deltaPosition.GetMagnitudeOnAxis(transform.right);
					break;
			}
			speed /= Time.deltaTime;
			deltas.Add(speed);
			if (deltas.Count >= numberBars)
			{
				deltas.RemoveAt(0);
			}
			lastPosition = currentPosition;
		}

		void OnGUI ()
		{
			var firstRect = new Rect((Screen.width * 0.5f) - (numberBars * 0.5f) * barWidthPercOfScreenWidth * 
									  Screen.width, Screen.height * 0.8f, Screen.width * barWidthPercOfScreenWidth,
									  Screen.height * 0.2f);
			for (var i = 0; i < numberBars; i++)
			{
				var rect = new Rect(firstRect);
				rect.x += i * (barWidthPercOfScreenWidth * Screen.width);
				rect.height = deltas.Count <= i ? 0 : -deltas[i] * barScale;
				GUI.DrawTexture(rect, texture);
			}
		
			var bottomMiddle = new Rect(Screen.width * 0.45f, Screen.height * 0.8f, Screen.width * 0.1f, Screen.height * 0.2f);
			var text = string.Format("Speed: {0:f2}\nForward: {1:f2}\nVertical: {2:f2}", speed, 
										deltaPosition.GetMagnitudeOnAxis(transform.forward  / Time.deltaTime),
										deltaPosition.GetMagnitudeOnAxis(transform.up)  / Time.deltaTime);
			GUI.Label(bottomMiddle, text);
		}
	}
}
