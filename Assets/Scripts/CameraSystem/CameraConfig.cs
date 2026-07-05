using UnityEngine;

namespace MeatMushrooms.CameraSystem
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "MeatMushrooms/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Слежение за игроком")]
        public Vector3 FollowOffset = new Vector3(0f, 10f, -8f);
        public float PitchAngle = 50f;
        public float FollowSpeed = 5f;
    }
}