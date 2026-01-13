using NUnit.Framework;
using UnityEngine;

namespace ASG1.Tests.EditMode
{

    public class UtilitiesTests
    {
        [Test]
        public void Sample_TestAlwaysPasses()
        {

            Assert.IsTrue(true, "This test should always pass");
        }

        [Test]
        public void Vector3_Normalized_HasMagnitudeOne()
        {

            var vector = new Vector3(3, 4, 0);
            var normalized = vector.normalized;

            Assert.AreEqual(1f, normalized.magnitude, 0.0001f,
                "Normalized vector should have magnitude of 1");
        }

        [Test]
        public void Mathf_Clamp_ReturnsValueWithinRange()
        {

            float result = Mathf.Clamp(150f, 0f, 100f);

            Assert.AreEqual(100f, result, "Value above max should be clamped to max");
        }

    }
}
