using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMove : StateMachineBehaviour
{
    private Vector3 nextPosition = Vector3.zero;
    private float journeyLength;
    private float speed;
    private Transform parentTransform;
    private float startTime;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        nextPosition = GameScripts.SharedInstance.Factory.GetNextPosition(GameScripts.SharedInstance.LastMoveDirection);
        nextPosition = new Vector3(nextPosition.x, 1, nextPosition.z);
        parentTransform = animator.gameObject.transform.parent;
        journeyLength = Vector3.Distance(parentTransform.position, nextPosition);
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        float currentClipLength = clipInfos[0].clip.length;
        
        speed = (journeyLength / currentClipLength) * 2;
        startTime = Time.time;
        
        Debug.Log(nextPosition);
        Debug.Log(parentTransform.position);
        Debug.Log(journeyLength);
        Debug.Log(currentClipLength);
        Debug.Log(animator.speed);
        Debug.Log(speed);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        parentTransform.position = Vector3.Lerp(parentTransform.position, nextPosition, fractionOfJourney);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("move", true);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
