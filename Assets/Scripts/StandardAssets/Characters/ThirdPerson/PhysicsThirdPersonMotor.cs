﻿using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// The main third person controller
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class PhysicsThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		/// <summary>
		/// Movement values
		/// </summary>
		public Transform cameraTransform;

		public float maxForwardSpeed = 10f;
		public bool useAcceleration = true;
		public float groundAcceleration = 20f;
		public float groundDeceleration = 15f;

		[Range(0f, 1f)]
		public float airborneAccelProportion = 0.5f;

		[Range(0f, 1f)]
		public float airborneDecelProportion = 0.5f;
		public float jumpSpeed = 15f;
		
		public bool interpolateTurning = true;
		public float turnSpeed = 500f;

		[Range(0f, 1f)]
		public float airborneTurnSpeedProportion = 0.5f;

		/// <summary>
		/// The input implementation
		/// </summary>
		ICharacterInput m_CharacterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		ICharacterPhysics m_CharacterPhysics;

		/// <inheritdoc />
		public float turningSpeed { get; private set;}

		/// <inheritdoc />
		public float lateralSpeed { get; private set; }

		/// <inheritdoc />
		public float forwardSpeed
		{
			get
			{
				//Debug.Log("Forward Speed: "+currentForwardSpeed / maxForwardSpeed);
				return currentForwardSpeed / maxForwardSpeed;
			}
		}

		/// <summary>
		/// Fires when the jump starts
		/// </summary>
		public Action jumpStarted { get; set; }

		/// <summary>
		/// Fires when the player lands
		/// </summary>
		public Action landed { get; set; }

		float currentForwardSpeed;


		private float finalTargetRotation;
		private float currentRotation;
	
		
		
		/// <summary>
		/// Gets required components
		/// </summary>
		void Awake()
		{
			m_CharacterInput = GetComponent<ICharacterInput>();
			m_CharacterPhysics = GetComponent<ICharacterPhysics>();
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		void OnEnable()
		{
			m_CharacterInput.jumpPressed += OnJumpPressed;
			m_CharacterPhysics.landed += OnLanding;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		void OnDisable()
		{
			if (m_CharacterInput != null)
			{
				m_CharacterInput.jumpPressed -= OnJumpPressed;
			}

			if (m_CharacterPhysics != null)
			{
				m_CharacterPhysics.landed -= OnLanding;
			}
		}

		/// <summary>
		/// Handles player landing
		/// </summary>
		void OnLanding()
		{
			if (landed != null)
			{
				landed();
			}
		}

		/// <summary>
		/// Subscribes to the Jump action on input
		/// </summary>
		void OnJumpPressed()
		{
			if (m_CharacterPhysics.isGrounded)
			{
				m_CharacterPhysics.SetJumpVelocity(jumpSpeed);
				if (jumpStarted != null)
				{
					jumpStarted();
				}
			}
		}

		/// <summary>
		/// Movement Logic on physics update
		/// </summary>
		void FixedUpdate()
		{
			SetForward();
			CalculateForwardMovement();
			Move();
		}

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		void SetForward()
		{
			if (!m_CharacterInput.hasMovementInput)
			{
				return;
			}

			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);

			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);
			
			
			
			if (interpolateTurning)
			{
				//ADDED IN PREVIOUS ROTATION
				Quaternion previousTargetRotation = targetRotation;
				
				
				float actualTurnSpeed =
					m_CharacterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
				targetRotation =
					Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.deltaTime);
				
				/*
				//TS set to DIFFERENCE between last and next. 
				//This is probably wrong...
				*/
				turningSpeed = previousTargetRotation.y - targetRotation.y;
			}
			currentRotation = transform.rotation.y;
			finalTargetRotation = targetRotation.y;
			
			transform.rotation = targetRotation;
			
			

		}

		void Update()
		{
			
			//turningSpeed = getTurningSpeed();
			
		}

		float getTurningSpeed()
		{
			//If the last rotated angle is not "about" the same as the target
			//Then it works out the change in angle / speed
			//Then it updates the last known angle.
			//IF the current angle and the target ar teh  same, then it will return the 
			//difference, which will be 0.
			if (Math.Abs(currentRotation - finalTargetRotation) > 0.05)
			{	
				float speed = (transform.rotation.y - currentRotation) / Time.deltaTime;
				currentRotation = transform.rotation.y;
				return speed;
			}	
			return (transform.rotation.y - finalTargetRotation)/Time.deltaTime;
		}
		

		/// <summary>
		/// Calculates the forward movement
		/// </summary>
		void CalculateForwardMovement()
		{
			Vector2 moveInput = m_CharacterInput.moveInput;
			if (moveInput.sqrMagnitude > 1f)
			{
				moveInput.Normalize();
			}

			
			float desiredSpeed = moveInput.magnitude * maxForwardSpeed;

			if (useAcceleration)
			{
				float acceleration = m_CharacterPhysics.isGrounded
					? (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
					: (m_CharacterInput.hasMovementInput ? groundAcceleration : groundDeceleration) *
					  airborneDecelProportion;

				currentForwardSpeed =
					Mathf.MoveTowards(currentForwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
			}
			else
			{
				currentForwardSpeed = desiredSpeed;
			}
		}

		/// <summary>
		/// Moves the character
		/// </summary>
		void Move()
		{
			Vector3 movement;
			//TODO: clean-up
//			if (m_IsGrounded && m_Animator.deltaPosition.z >= groundAcceleration * Time.deltaTime)
//			{
//				RaycastHit hit;
//				Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
//				if (Physics.Raycast (ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
//				{
//					movement = Vector3.ProjectOnPlane (m_Animator.deltaPosition, hit.normal);
//				}
//				else
//				{
//					movement = m_Animator.deltaPosition;
//				}
//			}
//			else
//			{
			movement = currentForwardSpeed * transform.forward * Time.deltaTime;
//			}

			//movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

			m_CharacterPhysics.Move(movement);
		}
	}
}