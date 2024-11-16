using System;
using System.Collections;
using UnityEngine;
using RSM;

/*
Classe que controla a fisica do foguete
A state machine e os elementos de UI se comunicam com essa classe
Gravidade eh aplicada dentro dessa classe
*/
public class RocketController : MonoBehaviour
{
    public bool isActive = false;
    public float thrustLevel = 0f;
    [HideInInspector] public float currentFuelMass;
    public float maxFuelMass = 700, fuelMassExpulsionPerSecond = 70, fuelExpulsionVelocity = 1000;
    public float maxTurnSpeed = 40;

    public Transform nextStage;
    public bool isDetached => nextStage == null;
    public bool autoPilotOnDetach = false;

    [SerializeField] private Transform thruster;
    public Transform target;
    public float targetAltitude = 300;
    private Rigidbody thisStage;
    private float massOfAllNextStages;
    private float aggregatedMass => thisStage.mass + massOfAllNextStages;

    public Vector3 velocity { get => thisStage.velocity; set => thisStage.velocity = value; }
    private ParticleSystem[] thrusterParticleSystems;
    private bool emmitingParticles = false;

    void OnEnable()
    {
        thrusterParticleSystems = thruster.GetComponentsInChildren<ParticleSystem>();
        StopParticleSystems();
        currentFuelMass = maxFuelMass;
        thisStage = GetComponent<Rigidbody>();

        massOfAllNextStages = 0;
            foreach(Rigidbody part in GetComponentsInChildren<Rigidbody>())
                massOfAllNextStages += part.mass;
    }

    private void StartParticleSystems()
    {
        foreach(ParticleSystem particleSystem in thrusterParticleSystems) particleSystem.Play();
    }

    private void StopParticleSystems()
    {
        foreach(ParticleSystem particleSystem in thrusterParticleSystems) particleSystem.Stop();
    }

    public void DetachNextStage()
    {
        if(nextStage == null) return;

        if(autoPilotOnDetach) thrustLevel = 1;

        nextStage.parent = null;
        nextStage.GetComponent<BoxCollider>().enabled = true;

        Rigidbody nextStageRb = nextStage.GetComponent<Rigidbody>();
        nextStageRb.isKinematic = false;
        nextStageRb.interpolation = RigidbodyInterpolation.Interpolate;
        nextStageRb.velocity = thisStage.velocity;

        RocketStateMachine rsm = nextStage.GetComponent<RocketStateMachine>();
        rsm.enabled = true;
        VirtualCameraControl.FocusOnCamera(rsm.virtualCameraIndex);

        nextStage.GetComponent<RocketController>().isActive = true;

        nextStage = null;
    }

    // Aqui a velocidade e o thurst eh aplicado
    void FixedUpdate()
    {
        if(isActive)
        {
            thisStage.AddForce(Vector3.up * -9.81f * Time.fixedDeltaTime, ForceMode.VelocityChange);
            ActivateThruster();
        }
            
    }

    public float GetCurrentMaximumThursterAcceleration()
    {
        float fuelMassExpelled = Mathf.Clamp(fuelMassExpulsionPerSecond * thrustLevel, 0, currentFuelMass);
        return fuelExpulsionVelocity * Mathf.Log((aggregatedMass + fuelMassExpelled) / aggregatedMass);
    }

    public void ActivateThruster()
    {
        float fuelMassExpelled = Mathf.Clamp(fuelMassExpulsionPerSecond * Time.fixedDeltaTime * thrustLevel, 0, currentFuelMass);
        currentFuelMass -= fuelMassExpelled;
        thisStage.mass -= fuelMassExpelled;
        thisStage.AddForce(
            // A forca eh o resultado da Tsiolkovsky Rocket Equation, que determina a mudanca velocidade
            thruster.up * (fuelExpulsionVelocity * Mathf.Log((aggregatedMass + fuelMassExpelled) / aggregatedMass)), 
        ForceMode.VelocityChange);

        if(fuelMassExpelled > 0)
        {
            if(!emmitingParticles) StartParticleSystems();
            emmitingParticles = true;
        }
        else
        {
            if(emmitingParticles) StopParticleSystems();
            emmitingParticles = false;
        } 
    }

    public void SetHorizontalVelocity(Vector3 velocity)
    {
        thisStage.velocity = Vector3.Project(thisStage.velocity, Vector3.up) + velocity;
    }

    public void Stop() { thisStage.velocity = Vector3.zero; thisStage.angularVelocity = Vector3.zero; }

    // Aplica torque pra apontar o foguete numa direcao desejada
    public void TurnToDirection(Vector3 direciton)
    {
        Vector3 targetAxis = Vector3.Cross(direciton, transform.up).normalized;

        float mainAngleDiff = Vector3.SignedAngle(transform.up, direciton, targetAxis);

        float targetVel = Mathf.Lerp(0, maxTurnSpeed, Mathf.Abs(mainAngleDiff)/180f) * Mathf.Sign(mainAngleDiff);
        Vector3 angVel_targetAxis = Mathf.Rad2Deg * Vector3.Project(thisStage.angularVelocity, targetAxis);
        Vector3 velDiff = targetVel * targetAxis - angVel_targetAxis;

        thisStage.angularVelocity = Mathf.Deg2Rad * angVel_targetAxis;
        // Num foguete real, a rotacao acontece atraves da rotacao do propulsor
        // Entao rotaciono o propulsor pra parecer mais real, mas o torque eh aplicado diretamente para
        // simplificar o calculo pra atingir a velocidade angular desejada
        thruster.transform.localRotation = Quaternion.AngleAxis(Mathf.Clamp(mainAngleDiff, -30, 30), targetAxis) * Quaternion.identity;

        ApplyTorque(velDiff);
    }

    private void ApplyTorque(Vector3 torque)
    {
        thisStage.AddTorque(transform.right * torque.x * Time.fixedDeltaTime, ForceMode.Acceleration);
        thisStage.AddTorque(transform.up * torque.y * Time.fixedDeltaTime, ForceMode.Acceleration);
        thisStage.AddTorque(transform.forward * torque.z * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

}
