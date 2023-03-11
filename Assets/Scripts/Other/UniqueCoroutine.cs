using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Other
{
    internal class UniqueCoroutine : MonoBehaviour
    {
        public IEnumerator Coroutine { get; set; }

        bool hasBegun = false;

        public UniqueCoroutine()
        {

        }

        public void Begin()
        {
            if (Coroutine == null)
            {
                throw new NullReferenceException("Coroutine is null");
            }

            if (!hasBegun)
            {
                hasBegun = true;
                StartCoroutine(Coroutine);
            }
        }

        public void End()
        {
            if (Coroutine == null)
            {
                throw new NullReferenceException("Coroutine is null");
            }

            if (hasBegun)
            {
                StopCoroutine(Coroutine);
                hasBegun = false;
            }
        }

        public void Finish()
        {
            if (Coroutine == null)
            {
                throw new NullReferenceException("Coroutine is null");
            }

            if (hasBegun)
            {
                hasBegun = false;
            }
        }
    }
}
