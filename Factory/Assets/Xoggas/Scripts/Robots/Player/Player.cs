﻿using System;
using UnityEngine;

namespace MelonJam4.Factory
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Player : Robot
    {
        public event Action<float, float> OnCompromiseTimerUpdate;
        public event Action OnCompromised;

        [Header("Player Properties")]
        [SerializeField]
        private float _movementSpeed = 5f;

        [SerializeField]
        private float _rotationSpeed = 5f;

        [SerializeField]
        private float _timeLimit = 2f;

        #region RuntimeVariables

        private Rigidbody _rigidbody;
        private float _compromisedTimer;
        private bool _isInStealthMode;
        private bool _isBeingCompromised;

        #endregion

        #region Unity

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Level.Instance.IsGameRunning == false)
            {
                return;
            }

            Transform();
            UpdateCompromisedTimer();
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.TryGetComponent<Policeman>(out _))
            {
                _isBeingCompromised = true;
            }
        }

        private void OnTriggerStay(Collider collision)
        {
            if (collision.TryGetComponent<Policeman>(out _))
            {
                _isBeingCompromised = true;
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.TryGetComponent<Policeman>(out _))
            {
                _isBeingCompromised = false;
            }
        }

        #endregion

        private Vector3 GetInputVector()
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
            var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            return matrix.MultiplyPoint(input);
        }

        private void Transform()
        {
            var input = GetInputVector();

            if (input.magnitude <= 0.00001f)
            {
                _rigidbody.velocity = Vector3.zero;
                return;
            }

            _rigidbody.velocity = input * _movementSpeed;

            var rotation = Quaternion.LookRotation(input, Vector3.up);

            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotation, Time.deltaTime * _rotationSpeed);
        }

        private void UpdateCompromisedTimer()
        {
            if (_isInStealthMode)
            {
                return;
            }

            if (_isBeingCompromised)
            {
                _compromisedTimer += Time.deltaTime;
            }
            else if (_compromisedTimer > 0f)
            {
                _compromisedTimer -= Time.deltaTime;
            }
            else
            {
                _compromisedTimer = 0f;
            }

            OnCompromiseTimerUpdate?.Invoke(_compromisedTimer, _timeLimit);

            if (_compromisedTimer >= _timeLimit)
            {
                OnCompromised?.Invoke();
            }
        }
    }
}
