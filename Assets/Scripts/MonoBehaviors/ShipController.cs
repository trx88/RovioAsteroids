using RovioAsteroids.MonoBehaviors.GameObjectFactory.Abstraction;
using RovioAsteroids.Repository.Items.DataModels;
using RovioAsteroids.Repository.Repositories.Abstraction;
using RovioAsteroids.Repository.Repositories.RepositoryFactories;
using RovioAsteroids.Utils;
using System.Linq;
using UnityEngine;
using Zenject;

namespace RovioAsteroids.MonoBehaviors
{
    /// <summary>
    /// Controls the player ship.
    /// </summary>
    public class ShipController : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 100.0f;
        [SerializeField] private float _thrustForce = 3f;
        [SerializeField] private float _firingAngle = 15f;
        [SerializeField] private AudioClip _soundCrash = default;
        [SerializeField] private AudioClip _soundShoot = default;
        [SerializeField] private GameObject _laser = default;
        [SerializeField] private Transform _gunSystem = default;

        private IRepository<GameSessionData> _gameSessionDataRepository;
        private ILaserFactory _laserFactory;

        [Inject]
        private void Construct(
            InMemoryRepositoryFactory inMemoryRepositoryFactory,
            ILaserFactory laserFactory)
        {
            _gameSessionDataRepository = inMemoryRepositoryFactory.RepositoryOf<GameSessionData>();
            _laserFactory = laserFactory;
        }

        void FixedUpdate()
        {
            //Rotate ship
            transform.Rotate(0, 0, -Input.GetAxis(StaticStrings.Axis_Horizontal) *
                _rotationSpeed * Time.deltaTime);

            //Thrust ship
            GetComponent<Rigidbody2D>().
                AddForce(transform.up * _thrustForce *
                    Input.GetAxis(StaticStrings.Axis_Vertical));
        }

        private void Update()
        {
            //Moved to regular Update since shooting requires more frequent calls.
            if (Input.GetMouseButtonDown(0))
            {
                //ShootOne();
                ShootThree();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != StaticStrings.Tag_Laser)
            {
                var particles = GetComponentInChildren<ParticleSystem>();
                if(particles != null)
                {
                    particles.Play();
                }    

                AudioSource.PlayClipAtPoint(_soundCrash, Camera.main.transform.position);

                //Reset ship
                transform.position = new Vector3(0, 0, 0);
                GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);

                //Reduce lives
                GameSessionData gameSessionData = _gameSessionDataRepository.Get(x => true).Single();
                gameSessionData.Lives--;
                _gameSessionDataRepository.Update(gameSessionData);
            }
        }

        private void ShootOne()
        {
            _laserFactory.CreateLaser(
                _gunSystem.position,
                transform.rotation);

            AudioSource.PlayClipAtPoint(_soundShoot, Camera.main.transform.position);
        }

        private void ShootThree()
        {
            _laserFactory.CreateLaser(
                _gunSystem.position, 
                ModifyQuaternionWithEuler(transform.rotation, new Vector3(0f, 0f, -_firingAngle)));

            _laserFactory.CreateLaser(
                _gunSystem.position,
                transform.rotation);

            _laserFactory.CreateLaser(
                _gunSystem.position,
                ModifyQuaternionWithEuler(transform.rotation, new Vector3(0f, 0f, _firingAngle)));

            AudioSource.PlayClipAtPoint(_soundShoot, Camera.main.transform.position);
        }

        private Quaternion ModifyQuaternionWithEuler(Quaternion rotation, Vector3 euler)
        {
            return Quaternion.Euler(rotation.eulerAngles + euler);
        }
    }
}
