using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class FaceDirectionTest
    {

        private readonly Vector3 _upRightDir = new (20, 30, 0);
        private readonly Vector3 _leftDownDir = new (-30, -20, 0);
        
        [Test]
        public void UpRight1()
        {
            Assert.AreEqual(Vector2.right, Utils.GetNextFaceDirection(Vector2.up, _upRightDir));
        }
        
        [Test]
        public void UpRight2()
        {
            Assert.AreEqual(Vector2.left, Utils.GetNextFaceDirection(Vector2.right, _upRightDir));
        }
        
        [Test]
        public void UpRight3()
        {
            Assert.AreEqual(Vector2.down, Utils.GetNextFaceDirection(Vector2.left, _upRightDir));
        }
        
        [Test]
        public void UpRight4()
        {
            Assert.AreEqual(Vector2.zero, Utils.GetNextFaceDirection(Vector2.down, _upRightDir));
        }

        [Test]
        public void LeftDown1()
        {
            Assert.AreEqual(Vector2.down, Utils.GetNextFaceDirection(Vector2.left, _leftDownDir));
        }

        [Test]
        public void LeftDown2()
        {
            Assert.AreEqual(Vector2.right, Utils.GetNextFaceDirection(Vector2.up, _leftDownDir));
        }
    
    }
}
