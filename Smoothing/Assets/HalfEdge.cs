public class HalfEdge
{
    //also stores vertex start, just for ease of opposite calculations
    public int vertexStart;
    public int vertexEnd;
    public int face;
    public HalfEdge nextHalfEdge;
    public HalfEdge oppositeHalfEdge;
    public HalfEdge previousHalfEdge;
}