#include <iostream>
#include <vector>
using namespace std;

const int MAXN = 1000;
int ncycle = 0;
bool CycleFound;
vector<int> cycle[MAXN];
vector<int> g[MAXN];
vector<int> color;
vector<int> p;
int n;

int add_cycle(int cycle_end, int cycle_st)
{
	cycle[ncycle].clear();
	cycle[ncycle].push_back(cycle_st);
	for (int v = cycle_end; v != cycle_st; v = p[v])
		cycle[ncycle].push_back(v);
	cycle[ncycle].push_back(cycle_st);

	reverse(cycle[ncycle].begin(), cycle[ncycle].end());

	return cycle[ncycle].size();
}
void dfs(int v)
{
	color[v] = 1;
	for (int i = 0; i < g[v].size(); i++)
	{
		int to = g[v][i];
		if (color[to] == 0)
		{
			p[to] = v;
			dfs(to);
		}
		else if (color[to] == 1)
		{
			CycleFound = true;
			if (add_cycle(v, to) > 3) // Исключение вырожденных случаев, н: 1 2 1
				ncycle++;
		}
	}
	color[v] = 0; // Исправлено. Было: color[v] = 2;
}
void find_cycles()
{
	for (int i = 0; i < n; i++)
	if (color[i] == 0)
		dfs(i);
}

int main()
{
	n = 4;
	color.assign(n, 0);
	p.assign(n, 0);
	g[0].push_back(1); g[0].push_back(2);
	g[1].push_back(2); g[1].push_back(3);
	g[2].push_back(3);

	g[1].push_back(0); g[2].push_back(0);
	g[2].push_back(1); g[3].push_back(1);
	g[3].push_back(2);

	find_cycles();

	for (int i = 0; i < ncycle; ++i)
	{
		for (size_t j = 0; j < cycle[i].size(); ++j)
		{
			cout << cycle[i][j] << " ";
		}
		cout << "\n";
	}
	system("PAUSE");
	return 0;
}