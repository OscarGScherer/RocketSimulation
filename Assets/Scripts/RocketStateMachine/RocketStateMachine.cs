using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*

Para simplificar o movimento to foguete, uso uma State Machine para separar
o caminho do foguete em etapas. Como essa State Machine eh bem simples, coloquei toda logica
nesse arquivo.
A descricao dos 3 estagios do foguete esta estabelecida no final desse arquivo.
*/
namespace RSM{

// Base pra todos os estados do foguete
public abstract class State
{
    // funcao base de cada estado, retorna o proximo estado se precisa ser alterado
    public abstract State Process(RocketController rc);
    public virtual void OnEnter(RocketController rc) { return; }
    public virtual void OnExit(RocketController rc) { return; }
    public virtual void OnCollisionEnter(Collision collision, RocketController rc) { return; }
}

// Definicao do component da State Machine
public class RocketStateMachine : MonoBehaviour
{
    public int virtualCameraIndex = -1;
    private RocketController rc;

    public State currentState;

    public void OnEnable()
    {
        rc = GetComponent<RocketController>();
        currentState = new GainAltitude();
        currentState.OnEnter(rc);
    }

    public void FixedUpdate() => Evaluate();

    public void Evaluate()
    {
        if(currentState == null) return;

        State newState = currentState.Process(rc);

        // Caso o estado do foguete foi alterado, chamar onExit e onEnter
        if (newState?.GetType() != currentState.GetType())
        {
            currentState.OnExit(rc);
            newState?.OnEnter(rc);
            currentState = newState;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Terrain")
        {
            if(!GameState.failed)
            {
                VirtualCameraControl.FocusOnCamera(virtualCameraIndex);
                GameState.failReason = name + " fail to land";
                GameState.failed = true;
            }
        }
        else currentState?.OnCollisionEnter(collision, rc);
    }
}

    // A primeira etapa envolve somente atingir uma altituda desejada
    // Bem simples
    public class GainAltitude : State
    {
        protected float startAltitude;

        public override void OnEnter(RocketController rc) => startAltitude = rc.transform.position.y;

        public override State Process(RocketController rc)
        {
            rc.TurnToDirection(Vector3.up);
            if(rc.transform.position.y > rc.targetAltitude) return new FlyToTarget();

            return this;
        }
    }

    // A segunda etapa envole fazer o foguete voar ate o local de pouso
    public class FlyToTarget : State
    {

        public override State Process(RocketController rc)
        {
            Vector3 dirToTarget = rc.target.position - rc.transform.position;
            Vector3 horizontalDirToTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
            
            Vector3 perp = Vector3.Cross(horizontalDirToTarget.normalized, Vector3.up);

            // aqui eh calculado o tempo de queda se a propulsao fosse desligada
            float yVel = -rc.velocity.y;
            float height = Mathf.Abs(dirToTarget.y) - 30; // 30 eh mais ou menos a distancia do pivot do foguete ate o propulsor pra calcular a altura corretamente
            float fallTime = (-yVel + Mathf.Sqrt(Mathf.Pow(yVel, 2) + 2 * 9.81f * height)) / 9.81f;
            Vector3 minimumVelocityVector = horizontalDirToTarget/fallTime;
            float minimumVelocity = minimumVelocityVector.magnitude;

            // Aqui a velocidade horizontal eh limitada com base no tempo de queda
            // para q a velocidade nao fique alta demais para freiar
            if(Vector3.Project(rc.velocity, minimumVelocityVector).magnitude >= minimumVelocity)
                rc.SetHorizontalVelocity(minimumVelocityVector);
            if(horizontalDirToTarget.magnitude < 500 && rc.isDetached) return new Land();

            // Aqui eh calculado a aceleracao maxima no nivel atual de thrust, para depois calcular o angulo necessario
            // pra manter a velocity y do foguete em 0
            float maximumAcceleration = rc.GetCurrentMaximumThursterAcceleration();
            float oppositeOverHipotenuse = Mathf.Clamp((9.81f + -rc.velocity.y/fallTime) / maximumAcceleration, -1, 1);
            float angle = maximumAcceleration <= 0 ? 0 : Mathf.Rad2Deg * Mathf.Asin(oppositeOverHipotenuse);
            angle = Mathf.Clamp(angle, -45, 45);

            Vector3 direction = Quaternion.AngleAxis(angle, perp) * horizontalDirToTarget.normalized;

            rc.TurnToDirection(direction);

            if(rc.transform.position.y < rc.targetAltitude) return new GainAltitude();

            return this;
        }
    }

    /*
    Processo de pouso bem simplificado para garantir que o foguete pouse automaticamente
    */
    public class Land : State
    {
        bool landed = false;

        public override void OnCollisionEnter(Collision collision, RocketController rc)
        {
            if(collision.gameObject.transform == rc.target) landed = true;
        }

        public override void OnExit(RocketController rc) => rc.Stop();

        public override State Process(RocketController rc)
        {
            if(landed) return null;

            rc.TurnToDirection(Vector3.up);

            Vector3 dirToTarget = rc.target.position - rc.transform.position;
            Vector3 horizontalVectorToTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
            float yVel = -rc.velocity.y;
            float height = Mathf.Abs(dirToTarget.y) - 30;
            float fallTime = (-yVel + Mathf.Sqrt( Mathf.Pow(yVel, 2) + 2 * 9.81f * height)) / 9.81f;

            // Aqui a velocidade horizontal eh mantida no valor ideal pra pousar corretamente automaticamente
            // com base no tempo de queda e na distancia
            rc.SetHorizontalVelocity(horizontalVectorToTarget/fallTime);

            return this;
        }
    }
}
