using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
namespace GJPlus2023
{
    public class BulletController : MonoBehaviour
    {
        [FoldoutGroup("Bullet Controller")][SerializeField] private Rigidbody rb;
        [FoldoutGroup("Bullet Controller")][SerializeField] private float bulletTimeOut;
        [FoldoutGroup("Bullet Controller")] private ObjectPullingBullet bulletOP;
        [FoldoutGroup("Bullet Controller")] public Transform transObjectPulling;
        [FoldoutGroup("Bullet Controller")] private float _bulletTimeoutDelta;
        private void Update()
        {
            if (_bulletTimeoutDelta >= 0)
                _bulletTimeoutDelta -= Time.deltaTime;
            if (_bulletTimeoutDelta <= 0)
                MoveToObjectPulling();
        }
        public void SetUp(Transform trans)
        {
            transObjectPulling = trans;
            bulletOP = transObjectPulling.GetComponent<ObjectPullingBullet>();
            _bulletTimeoutDelta = bulletTimeOut;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 7)
            {
                MoveToObjectPulling();
            }
        }
        void MoveToObjectPulling()
        {
            bulletOP.listBullet.Add(gameObject);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            _bulletTimeoutDelta = bulletTimeOut;
            transform.SetParent(transObjectPulling);
        }
    }
}