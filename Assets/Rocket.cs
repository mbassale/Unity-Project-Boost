using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 750f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;
    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] float levelLoadDelay = 1.0f;


    const string FRIENDLY_TAG = "Friendly";
    const string FINISH_TAG = "Finish";

    enum State { Alive, Dying, Transcending };
    State state = State.Alive;
    bool collisionEnabled = true;

    Rigidbody rigidBody;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    { 
        if (state == State.Alive)
        {
            Thrust();
            Rotate();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadPreviousLevel();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionEnabled = !collisionEnabled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) { return; }
        if (!collisionEnabled) { return; }

        switch (collision.gameObject.tag)
        {
            case FRIENDLY_TAG:
                // do nothing
                break;
            case FINISH_TAG:
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.PlayOneShot(success);
        if (mainEngineParticles.isPlaying)
        {
            mainEngineParticles.Stop();
        }
        successParticles.Play();
        Invoke(nameof(LoadNextLevel), levelLoadDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.PlayOneShot(death);
        if (mainEngineParticles.isPlaying)
        {
            mainEngineParticles.Stop();
        }
        deathParticles.Play();
        Invoke(nameof(LoadFirstLevel), levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        int level = SceneManager.GetActiveScene().buildIndex;
        level++;
        if (level > 1)
        {
            level = 1;
        }
        SceneManager.LoadScene(level);
    }

    private void LoadPreviousLevel()
    {
        int level = SceneManager.GetActiveScene().buildIndex;
        level--;
        if (level < 0)
        {
            level = 0;
        }
        SceneManager.LoadScene(level);
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            ApplyThrust();
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if (mainEngineParticles.isPlaying)
            {
                mainEngineParticles.Stop();
            }
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }

    private void Rotate()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation
    }
}
