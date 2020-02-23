/*PATRICIO HIDALGO SANCHEZ. 2019*/

using UnityEngine;
using PathCreation.Utility;

[System.Serializable]
public class Stretch
{
  public enum CurveSide
  {
    A,
    B
  }
  #region IMPORTANT_POINTS
  private Vector3 md;
  public Vector3 MidPoint
  {
    get
    {
      return AnchorA.Pos + (AnchorB.Pos - AnchorA.Pos) * 0.5f;
    }
  }

  [SerializeField]
  public int anchorAIndex;
  private Node _anchorA;
  public Node AnchorA
  {
    get
    {
      Node a = NodeNetCreator.mainNet.GetNode(anchorAIndex);
      return a;
    }
  }

  [SerializeField]
  public int anchorBIndex;
  private Node _anchorB;
  public Node AnchorB
  {
    get
    {
       return NodeNetCreator.mainNet.GetNode(anchorBIndex); 
    }
  }

  public void NotifyDeleted(int i)
  {
    if (anchorAIndex > i)
    {
      --anchorAIndex;
      _anchorA = NodeNetCreator.mainNet.GetNode(anchorAIndex);
    }
      
    if (anchorBIndex > i)
    {
      --anchorBIndex;
      _anchorB = NodeNetCreator.mainNet.GetNode(anchorBIndex);
    }    
  }

  [SerializeField]
  public Vector3 cA;
  public Vector3 ControlA
  {
    get
    {
      return AnchorA.Pos + cA;
    }
    set
    {
      cA = value - AnchorA.Pos;
    }
  }

  [SerializeField]
  public Vector3 cB;
  public Vector3 ControlB
  {
    get
    {
      return AnchorB.Pos + cB;
    }
    set
    {
      cB = value - AnchorB.Pos;
    }
  }

  //public Bounds Bounds { get; private set; }
  #endregion

  public string Name
  {
    get
    {
      return "Stretch_" + AnchorA.id + "_" + AnchorB.id;
    }
  }

  /// <summary>
  /// This is an approximate value of the maximum curvature found in our Stretches.
  /// Used to define a range of curvatures and perfom operations as normalization.
  /// </summary>
  public static readonly float maxCurvature = 5f;

  #region COLLECTIONS
  private float[] _lengths;
  private float[] Lengths
  {
    get
    {
      if (_lengths == null || _lengths.Length == 0)
        RecalculateLengths();

      return _lengths;
    }
    set
    {
      _lengths = value;
    }
  }

  private float[] _verticesT;
  public float[] VerticesT
  {
    get
    {
      if (_verticesT == null || _verticesT.Length == 0)
      {
        RecalculateVertices();
        //RecalculateVertices(true);
      }
      return _verticesT;
    }
  }

  private Vector3[] _vertices;
  public Vector3[] Vertices
  {
    get
    {
      if (_vertices == null || _vertices.Length == 0)
      {
        RecalculateVertices();
        //RecalculateVertices(true);
      }
      return _vertices;
    }

    private set
    {
      _vertices = value;
    }
  }
  #endregion

  #region CONSTRUCTORS
  //public Stretch(Node a, Node b)
  //{
  //  anchorA = a;
  //  anchorB = b;

  //  cA = (anchorB.pos - anchorA.pos) * 0.333f;
  //  cB = (anchorA.pos - anchorB.pos) * 0.333f;
  //}

  public Stretch(int a, int b)
  {
    anchorAIndex = a;
    anchorBIndex = b;

    cA = (AnchorB.Pos - AnchorA.Pos) * 0.333f;
    cB = (AnchorA.Pos - AnchorB.Pos) * 0.333f;
  }

  public Stretch(int a, int b, Vector3 controlARelativePos, Vector3 controlBRelativePos)
  {
    anchorAIndex = a;
    anchorBIndex = b;



    ControlA = controlARelativePos;
    ControlB = controlBRelativePos;
  }

  #endregion

  #region OPERATORS
  public override bool Equals(object obj)
  {
    if ((obj == null) || !GetType().Equals(obj.GetType()))
    {
      return false;
    }
    else
    {
      Stretch st = (Stretch)obj;
      return ((st.AnchorA == AnchorA && st.AnchorB == AnchorB) ||
              (st.AnchorA == AnchorB && st.AnchorB == AnchorA));
    }
  }

  public override int GetHashCode()
  {
    return AnchorA.GetHashCode() ^ AnchorB.GetHashCode();
  }

  public static bool operator ==(Stretch st1, Stretch st2)
  {
    if (object.ReferenceEquals(st1, null))
    {
      if (ReferenceEquals(st2, null))
      {
        return true;
      }
      return false;
    }
    if (object.ReferenceEquals(st2, null))
    {
      if (ReferenceEquals(st1, null))
      {
        return true;
      }
      return false;
    }
    else
    {
      return ((st1.AnchorA == st2.AnchorA && st1.AnchorB == st2.AnchorB) ||
              (st1.AnchorA == st2.AnchorB && st1.AnchorB == st2.AnchorA));
    }
  }

  public static bool operator !=(Stretch st1, Stretch st2)
  {
    if (object.ReferenceEquals(st1, null))
    {
      if (ReferenceEquals(st2, null))
      {
        return false;
      }
      return true;
    }
    if (object.ReferenceEquals(st2, null))
    {
      if (ReferenceEquals(st1, null))
      {
        return false;
      }
      return true;
    }
    else
    {
      return !((st1.AnchorA == st2.AnchorA && st1.AnchorB == st2.AnchorB) ||
              (st1.AnchorA == st2.AnchorB && st1.AnchorB == st2.AnchorA));
    }
  }
  #endregion

  public void OnPathModified()
  {
    RecalculateLengths();
    RecalculateVertices();
    //RecalculateVertices(true);
  }

  public bool IsAnchorA(Node node)
  {
    if (node == AnchorA)
      return true;
    if (node == AnchorB)
      return false;
    Debug.Assert(true);
    return false;
  }

  public bool IsIntersection()
  {    
    IntersectionEntrance ieA = AnchorA.GetComponent<IntersectionEntrance>();
    IntersectionEntrance ieB = AnchorB.GetComponent<IntersectionEntrance>();

    if (ieA == null || ieB == null)
      return false;

    if (ieA.centre == ieB.centre)
    {
      return true;
    }

    return false;
  }

  #region GETTERS
  public float GetLength()
  {
    return Lengths[Lengths.Length - 1];
  }
  public Node GetOppositeNode(Node n)
  {
    if (n == AnchorA)
      return AnchorB;
    if (n == AnchorB)
      return AnchorA;

    return null;
  }
  public Vector3 GetPoint(float t)
  {
    return CubicBezierUtility.EvaluateCurve(GetPoints(), t);
  }
  public Vector3 GetTangent(float t)
  {
    return CubicBezierUtility.EvaluateCurveDerivative(GetPoints(), t).normalized;
  }
  /// <summary>
  /// 0 means no curvature. 1 means max curvature defined by <see cref="maxCurvature"/>
  /// </summary>
  /// <param name="t"></param>
  /// <returns></returns>
  public float GetCurvature(float t)
  {
    return Mathf.Clamp(CubicBezierUtility.EvaluateCurvature(GetPoints(), t), 0f, maxCurvature) / maxCurvature;
  }
  public Vector3 GetPointAtDistance(float d)
  {
    float t = AbsoluteDistanceToT(d);
    return GetPoint(t);
  }
  public Vector3 GetTangentAtDistance(float d)
  {
    float t = AbsoluteDistanceToT(d);
    return GetTangent(t);
  }
  public OrientedPoint GetOrientedPoint(float t)
  {
    return new OrientedPoint(GetPoint(t), GetTangent(t));
  }
  public OrientedPoint GetOrientedPointAtDistance(float d)
  {
    float t = AbsoluteDistanceToT(d);
    return new OrientedPoint(GetPoint(t), GetTangent(t));
  }
  /// <summary>
  /// 0 means no curvature. 1 means max curvature defined by <see cref="maxCurvature"/>
  /// </summary>
  /// <param name="d"></param>
  /// <returns></returns>
  public float GetCurvatureAtDistance(float d)
  {
    float t = AbsoluteDistanceToT(d);
    return Mathf.Clamp(CubicBezierUtility.EvaluateCurvature(GetPoints(), t), 0f, maxCurvature) / maxCurvature;
  }
  public Vector3 GetClosestPointOnStretch(Vector3 worldPos)
  {
    float t = ClosestPointT(worldPos);
    return GetPoint(t);
  }
  public Vector3 GetTangentAtClosestPoint(Vector3 worldPos)
  {
    float t = ClosestPointT(worldPos);
    return GetTangent(t);
  }
  public Vector3 GetTangentAtClosestPoint(Vector3 worldPos, bool forward, ref int index)
  {
    float t = ClosestPointT(worldPos, forward, ref index);
    return GetTangent(t);
  }
  public OrientedPoint GetClosestOrientedPoint(Vector3 worldPos, bool forward, ref int index)
  {
    float t = ClosestPointT(worldPos, forward, ref index);
    int sign = forward ? 1 : -1;
    return new OrientedPoint(GetPoint(t), GetTangent(t) * sign);
  }

  public OrientedPoint GetClosestOrientedPoint(Vector3 worldPos, bool forward)
  {
    float t = ClosestPointT(worldPos);
    int sign = forward ? 1 : -1;
    return new OrientedPoint(GetPoint(t), GetTangent(t) * sign);
  }

  /// <summary>
  /// anchorA, ControlA, ControlB, anchorB
  /// </summary>
  /// <returns></returns>
  public Vector3[] GetPoints()
  {
    Vector3[] points = new Vector3[4];

    points[0] = AnchorA.Pos;
    points[1] = ControlA;
    points[2] = ControlB;
    points[3] = AnchorB.Pos;

    return points;
  }

  public bool Contains(Node n)
  {
    return (AnchorA == n || AnchorB == n);
  }
  #endregion

  #region INTERNAL
  private int IndexOfLargestValueSmallerThan(float[] arcLengths, float targetArcLength)
  {
    int largest = 0;
    for (int i = 0; i < arcLengths.Length; i++)
    {
      if (arcLengths[i] <= targetArcLength)
        largest = i;
    }
    return largest;
  }
  private float LengthIndexToT(int i)
  {
    return (i / (float)(Lengths.Length - 1));
  }
  private void RecalculateLengths()
  {
    float curveLength = CubicBezierUtility.EstimateCurveLength(GetPoints());
    int nPoints = ((int)curveLength + 1) * 10;
    float[] l = new float[nPoints];
    l[0] = 0f;
    Vector3 lastPoint = GetPoint(0f);
    float accumulatedLength = 0f;
    for (int i = 1; i < nPoints; i++)
    {
      float t = 1f / nPoints * i;
      Vector3 p = GetPoint(t);
      float dst = Vector3.Distance(lastPoint, p);
      accumulatedLength += dst;
      l[i] = accumulatedLength;
      lastPoint = p;
    }
    _lengths = l;
  }
  private void RecalculateVertices()
  {
    float curveLength = GetLength();
    int nVertices = (int)(curveLength * 2 /*/ 5*/);
    Vector3[] v = new Vector3[nVertices];
    float[] tValues = new float[nVertices];

    float deltaDst = curveLength / (nVertices - 1);
    float d = 0f;
    for (int i = 0; i < nVertices; i++)
    {
      tValues[i] = AbsoluteDistanceToT(d);
      v[i] = GetPoint(tValues[i]);
      d += deltaDst;
    }

    _vertices = v;
    _verticesT = tValues;
  }

  private static float maxVerticesPerUnitLength = 0.2f;
  private void RecalculateVertices(bool useCurvature)
  {
    float curveLength = GetLength();
    int nVertices = 0;
    //if (useCurvature)
    //{
    //  int maxVertices = (int)(curveLength * maxVerticesPerUnitLength);
    //  float curvature = CubicBezierUtility.AproximateAmountOfCurvature(GetPoints()) / 100f;
    //  nVertices = Mathf.Max(2, Mathf.RoundToInt(Mathf.Lerp(0, maxVertices, curvature)));
    //}
    //else
    //{    
      nVertices = (int)(curveLength * 2 /*/ 5*/);    
    //}
    Vector3[] v = new Vector3[nVertices];
    float[] tValues = new float[nVertices];

    float deltaDst = curveLength / Mathf.Max(1, (nVertices - 1));
    float d = 0f;
    for (int i = 0; i < nVertices; i++)
    {
      tValues[i] = AbsoluteDistanceToT(d);
      v[i] = GetPoint(tValues[i]);
      d += deltaDst;
    }

    _vertices = v;
    _verticesT = tValues;
  }

  private float AbsoluteDistanceToT(float d)
  {
    d = Mathf.Clamp(d, 0f, GetLength());
    int i = IndexOfLargestValueSmallerThan(Lengths, d);
    if (d == Lengths[i])
      return LengthIndexToT(i);
    int j = i + 1;
    float LengthI = Lengths[i];
    float LengthJ = Lengths[j];
    float dif = LengthJ - LengthI;
    return Mathf.Lerp(LengthIndexToT(i), LengthIndexToT(j), (d - LengthI) / dif);
  }
  public float ClosestPointT(Vector3 worldPos)
  {
    int closestVertex = 0;
    int secondClosestVertex = 0;

    float minDistance = float.MaxValue;
    float minSecondDistance = float.MaxValue;

    for (int i = 0; i < Vertices.Length; i++)
    {
      float d = (Vertices[i] - worldPos).sqrMagnitude;
      if (d < minDistance)
      {
        secondClosestVertex = closestVertex;
        closestVertex = i;
        minSecondDistance = minDistance;
        minDistance = d;
      }
      else if (d < minSecondDistance)
      {
        secondClosestVertex = i;
        minSecondDistance = d;
      }
      else if (d > minSecondDistance)
        break;
    }

    Vector3 a = Vertices[closestVertex];
    Vector3 b = Vertices[secondClosestVertex];
    Vector3 c = MathUtility.ClosestPointOnLineSegment(worldPos, a, b);

    float lerpT = Vector3.Distance(c, a) / Vector3.Distance(b, a);

    float first = VerticesT[closestVertex];
    float second = VerticesT[secondClosestVertex];
    float t = Mathf.Lerp(first, second, lerpT);
    return t;
  }
  public float ClosestPointT(Vector3 worldPos, bool forward, ref int index)
  {
    int closestVertex = 0;
    int secondClosestVertex = 0;

    float minDistance = float.MaxValue;
    float minSecondDistance = float.MaxValue;

    if(forward)
    {
      for (int i = index; i < Vertices.Length; i++)
      {
        float d = (Vertices[i] - worldPos).sqrMagnitude;
        if (d < minDistance)
        {
          secondClosestVertex = closestVertex;
          closestVertex = i;
          minSecondDistance = minDistance;
          minDistance = d;
        }
        else if (d < minSecondDistance)
        {
          secondClosestVertex = i;
          minSecondDistance = d;
        }
        else
          break;
      }

      index = Mathf.Min(closestVertex, secondClosestVertex);
    }
    else
    {
      for (int i = index; i > 0; i--)
      {
        float d = (Vertices[i] - worldPos).sqrMagnitude;
        if (d < minDistance)
        {
          secondClosestVertex = closestVertex;
          closestVertex = i;
          minSecondDistance = minDistance;
          minDistance = d;
        }
        else if (d < minSecondDistance)
        {
          secondClosestVertex = i;
          minSecondDistance = d;
        }
        else
          break;
      }
      index = Mathf.Max(closestVertex, secondClosestVertex);
    }

    Vector3 a = Vertices[closestVertex];
    Vector3 b = Vertices[secondClosestVertex];
    Vector3 c = MathUtility.ClosestPointOnLineSegment(worldPos, a, b);

    float lerpT = Vector3.Distance(c, a) / Vector3.Distance(b, a);

    float first = VerticesT[closestVertex];
    float second = VerticesT[secondClosestVertex];
    float t = Mathf.Lerp(first, second, lerpT);
    return t;
  }
  #endregion
}
