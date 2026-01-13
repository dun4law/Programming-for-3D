using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ASG1.Tests.PlayMode
{

    public class PlaneTests
    {
        [UnityTest]
        public IEnumerator Sample_TestWaitsOneFrame()
        {

            yield return null;

            Assert.IsTrue(true, "This test should pass after one frame");
        }

        [UnityTest]
        public IEnumerator GameObjects_CanBeCreated()
        {

            var go = new GameObject("TestObject");

            yield return null;

            Assert.IsNotNull(go, "GameObject should be created");

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator Rigidbody_AppliesGravity()
        {

            var go = new GameObject("PhysicsTest");
            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = true;

            Vector3 startPos = go.transform.position;

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.Less(go.transform.position.y, startPos.y,
                "Rigidbody should fall due to gravity");

            Object.Destroy(go);
        }

    }
}
