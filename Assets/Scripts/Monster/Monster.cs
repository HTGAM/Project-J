using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] string playerTagName = "Player";
    [SerializeField] float findDistance;
    [SerializeField] float lerpSpeed;
    [SerializeField] float moveSpeed;
    [SerializeField] float attackDistance;
    public Transform target;

    void Update()
    {
        var find = Physics.OverlapBox(transform.position, Vector3.one * findDistance).FirstOrDefault(p => p.CompareTag(playerTagName));
        target = find == null ? null : find.transform;

        if (target != null)
        {
            Vector3 defaultRotation = Quaternion.LookRotation(target.transform.position - transform.position).eulerAngles;
            float fixedRotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, defaultRotation.y, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, fixedRotation, 0);

            float targetDistance = Vector3.Distance(transform.position, target.transform.position);
            if (targetDistance > attackDistance) transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

    }
}
